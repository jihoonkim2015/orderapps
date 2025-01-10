using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.IO;
using System.IO.Pipes;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace pos.wpf.worker
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IMongoCollection<Order> _orderCollection;
        private readonly IMongoCollection<Log> _logCollection;

        public Worker(ILogger<Worker> logger)
        {
            _logger = logger;
            var client = new MongoClient("mongodb://localhost:27017");
            var database = client.GetDatabase("OrderDatabase");
            _orderCollection = database.GetCollection<Order>("Orders");
            _logCollection = database.GetCollection<Log>("Logs");

        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await ListenForOrders(stoppingToken);
                await ListenForLogs(stoppingToken);
            }
        }

        private async Task ListenForOrders(CancellationToken stoppingToken)
        {
            using (var pipeServer = new NamedPipeServerStream("ProcOrderPipe", PipeDirection.In, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous))
            {
                await pipeServer.WaitForConnectionAsync(stoppingToken);

                using (var reader = new StreamReader(pipeServer))
                {
                    var jsonOrder = await reader.ReadToEndAsync();
                    var order = JsonSerializer.Deserialize<Order>(jsonOrder);
                    await SaveOrder(order);
                    _logger.LogInformation("Order received and saved to database: {Order}", order);
                }
            }
        }

        private async Task ListenForLogs(CancellationToken stoppingToken)
        {
            using (var pipeServer = new NamedPipeServerStream("ProcLogPipe", PipeDirection.In, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous))
            {
                await pipeServer.WaitForConnectionAsync(stoppingToken);

                using (var reader = new StreamReader(pipeServer))
                {
                    var logData = await reader.ReadToEndAsync();
                    var log = JsonSerializer.Deserialize<Log>(logData);
                    await SaveLog(log);
                    _logger.LogInformation("Log received and saved to database: {Log}", log);
                }
            }
        }


        private async Task SaveOrder(Order order)
        {
            var existingOrder = await _orderCollection.Find(o => o.OrderId == order.OrderId).FirstOrDefaultAsync();
            if (existingOrder == null)
            {
                await _orderCollection.InsertOneAsync(order);
            }
            else
            {
                var filter = Builders<Order>.Filter.Eq(o => o.OrderId, order.OrderId);
                var update = Builders<Order>.Update.Set(o => o.Status, order.Status);
                await _orderCollection.UpdateOneAsync(filter, update);
            }
        }

        private async Task SaveLog(Log log)
        {
            await _logCollection.InsertOneAsync(log);
        }

    }

    public class Order
    {
        public ObjectId Id { get; set; }
        public string OrderId { get; set; }
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public int TableNumber { get; set; }
        public int OrderCount { get; set; }
        public DateTime OrderDate { get; set; }
        public string Status { get; set; }
    }

    public class Log
    {
        public ObjectId Id { get; set; }
        public string LogData { get; set; }
        public DateTime LogDate { get; set; }
    }

}
