using System;
using System.IO;
using System.IO.Pipes;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using MongoDB.Driver;
using NUnit.Framework;

namespace pos.wpf.worker.tests
{
    [TestFixture]
    public class WorkerTests : IDisposable
    {
        private Mock<ILogger<Worker>> _loggerMock;
        private Mock<IMongoCollection<Order>> _orderCollectionMock;
        private Mock<IMongoCollection<Log>> _logCollectionMock;
        //private Mock<IMongoDatabase> _databaseMock;
        //private Mock<IMongoDbContext> _dbContextMock;
        //private Worker _worker;
        //private NamedPipeServerStream _pipeServerOrder;
        //private NamedPipeServerStream _pipeServerLog;

        [SetUp]
        public void Setup()
        {
            _loggerMock = new Mock<ILogger<Worker>>();
            _orderCollectionMock = new Mock<IMongoCollection<Order>>();
            _logCollectionMock = new Mock<IMongoCollection<Log>>();
            //_databaseMock = new Mock<IMongoDatabase>();
            //_dbContextMock = new Mock<IMongoDbContext>();

            //_databaseMock.Setup(db => db.GetCollection<Order>("Orders", null)).Returns(_orderCollectionMock.Object);
            //_databaseMock.Setup(db => db.GetCollection<Log>("Logs", null)).Returns(_logCollectionMock.Object);
            //_dbContextMock.Setup(db => db.Database).Returns(_databaseMock.Object);

            //_worker = new Worker(_loggerMock.Object, _dbContextMock.Object);

            //_pipeServerOrder = new NamedPipeServerStream("OrderPipe", PipeDirection.InOut, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);
            //_pipeServerLog = new NamedPipeServerStream("LogPipe", PipeDirection.InOut, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);
        }

        [TearDown]
        public void TearDown()
        {
            //_pipeServerOrder?.Dispose();
            //_pipeServerLog?.Dispose();
        }

        [Test]
        public async Task ListenForOrders_ShouldSaveOrder()
        {
            // Arrange
            var order = new Order
            {
                OrderId = Guid.NewGuid().ToString(),
                ProductName = "Sample Product",
                Quantity = 1,
                TableNumber = 1,
                OrderCount = 1,
                OrderDate = DateTime.Now,
                Status = "접수"
            };
            var jsonOrder = JsonSerializer.Serialize(order);

            using (var pipeClient = new NamedPipeClientStream(".", "ProcOrderPipe", PipeDirection.Out))
            {
                //await _pipeServerOrder.WaitForConnectionAsync();
                await pipeClient.ConnectAsync();

                using (var writer = new StreamWriter(pipeClient))
                {
                    await writer.WriteAsync(jsonOrder);
                }
                // Act
                //await _worker.ListenForOrders(CancellationToken.None);

                // Assert
                //_orderCollectionMock.Verify(c => c.InsertOneAsync(It.IsAny<Order>(), null, default), Times.Once);
            }
        }

        [Test]
        public async Task ListenForLogs_ShouldSaveLog()
        {
            // Arrange
            var log = new Log
            {
                LogData = new Order
                {
                    OrderId = Guid.NewGuid().ToString(),
                    ProductName = "Sample Product",
                    Quantity = 1,
                    TableNumber = 1,
                    OrderCount = 1,
                    OrderDate = DateTime.Now,
                    Status = "접수"
                },
                LogDate = DateTime.Now
            };
            var jsonLog = JsonSerializer.Serialize(log);

            using (var pipeClient = new NamedPipeClientStream(".", "ProcLogPipe", PipeDirection.Out))
            {
                //await _pipeServerLog.WaitForConnectionAsync();
                await pipeClient.ConnectAsync();
                using (var writer = new StreamWriter(pipeClient))
                {
                    await writer.WriteAsync(jsonLog);
                }

                // Act
                //await _worker.ListenForLogs(CancellationToken.None);

                // Assert
                //_logCollectionMock.Verify(c => c.InsertOneAsync(It.IsAny<Log>(), null, default), Times.Once);
                // Assert
                //_logCollectionMock.Verify(c => c.InsertOneAsync(It.IsAny<Log>(), null, default), Times.Once);
                //var insertedLog = _logCollectionMock.Invocations[0].Arguments[0] as Log;
                //Assert.NotNull(insertedLog);
                //Assert.AreEqual(log.LogData.OrderId, insertedLog.LogData.OrderId);
                //Assert.AreEqual(log.LogData.ProductName, insertedLog.LogData.ProductName);
                //Assert.AreEqual(log.LogData.Quantity, insertedLog.LogData.Quantity);
                //Assert.AreEqual(log.LogData.TableNumber, insertedLog.LogData.TableNumber);
                //Assert.AreEqual(log.LogData.OrderCount, insertedLog.LogData.OrderCount);
                //Assert.AreEqual(log.LogData.OrderDate, insertedLog.LogData.OrderDate);
                //Assert.AreEqual(log.LogData.Status, insertedLog.LogData.Status);
                //Assert.AreEqual(log.LogDate, insertedLog.LogDate);

            }
        }

        public void Dispose()
        {
            //_pipeServerOrder?.Dispose();
            //_pipeServerLog?.Dispose();
        }
    }
}
