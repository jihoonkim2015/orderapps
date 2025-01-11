using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Reflection.Emit;
using System.ServiceProcess;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace pos.wpf.worker
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IMongoDbContext _dbContext;
        private readonly IMongoCollection<Order> _orderCollection;
        private readonly IMongoCollection<Log> _logCollection;
        private const string serviceName = "pos.wpf.worker"; // 서비스 이름

        public Worker(ILogger<Worker> logger, IMongoDbContext dbContext)
        {
            _logger = logger;
            _dbContext = dbContext;
            _orderCollection = _dbContext.Database.GetCollection<Order>("Orders");
            _logCollection = _dbContext.Database.GetCollection<Log>("Logs");
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var orderProcessTask = ListenForOrders(stoppingToken);
            var logProcessTask = ListenForLogs(stoppingToken);
            var processCheckTask = CheckAndRestartProcessAsync(stoppingToken);

            await Task.WhenAll(new Task[] { orderProcessTask, logProcessTask, processCheckTask });
        }

        public async Task ListenForOrders(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
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
        }

        public async Task ListenForLogs(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using (var pipeServer = new NamedPipeServerStream("ProcLogPipe", PipeDirection.In, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous))
                {
                    await pipeServer.WaitForConnectionAsync(stoppingToken);

                    using (var reader = new StreamReader(pipeServer))
                    {
                        var logData = await reader.ReadToEndAsync();
                        var order = JsonSerializer.Deserialize<Order>(logData);
                        var log = new Log();
                        log.LogData = order;
                        log.LogDate = DateTime.Now;
                        await SaveLog(log);
                        _logger.LogInformation("Log received and saved to database: {Log}", log);
                    }
                }
            }
        }

        private async Task CheckAndRestartProcessAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                if (!IsServiceRunning(serviceName))
                {
                    StartService(serviceName);
                }
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }

        private bool IsServiceRunning(string serviceName)
        {
            var sc = new ServiceController(serviceName);
            return sc.Status == ServiceControllerStatus.Running;
        }

        private void StartService(string serviceName)
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = "sc",
                Arguments = $"start {serviceName}",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using (var process = Process.Start(startInfo))
            {
                process.WaitForExit();
                _logger.LogInformation($"Service {serviceName} restarted.");
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
        public Order LogData { get; set; }
        public DateTime LogDate { get; set; }
    }
}
