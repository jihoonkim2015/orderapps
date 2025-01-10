using System;
using System.IO;
using System.IO.Pipes;
using System.Text.Json;
using System.Threading.Tasks;

namespace post.order.dummy
{
    class Program
    {
        static readonly Random _random = new Random();

        static async Task Main(string[] args)
        {
            Console.WriteLine("Dummy order app started...");
            while (true)
            {
                await SendOrderRequest();
                
                //await Task.Delay(_random.Next(1000, 20000)); // 20초 대기 (1분에 3번 요청을 위해)
                await Task.Delay(100); // 20초 대기 (1분에 3번 요청을 위해)
            }

            Console.ReadLine();
        }
        volatile static int orderCount = 1;
        private static object lockObject = new object();

        static async Task SendOrderRequest()
        {
            var tablenumber = orderCount;
            var order = new Order
            {
                OrderId = Guid.NewGuid().ToString(),
                ProductName = "Sample Product",
                Quantity = 1,
                TableNumber =  tablenumber,
                //_random.Next(1, 5), // 테이블 번호를 1에서 4까지 랜덤하게 설정
                OrderCount = 3,
                OrderDate = DateTime.Now,
                Status = "접수"//GetRandomStatus() // 상태정보를 랜덤하게 설정
            }; 

            var jsonOrder = JsonSerializer.Serialize(order);

            using (var pipeClient = new NamedPipeClientStream(".", "OrderPipe", PipeDirection.Out))
            {
                await pipeClient.ConnectAsync();

                using (var writer = new StreamWriter(pipeClient))
                {
                    await writer.WriteAsync(jsonOrder);
                }
                // 로그 데이터 전송
                await SendLogData(jsonOrder);
            }

            Console.WriteLine($"Order sent at {DateTime.Now}: {jsonOrder}");


            if (orderCount > 10)
            {
                lock (lockObject) orderCount = 1;
            }
            await Task.CompletedTask;
        }

        static async Task SendLogData(string logData)
        {
            using (var pipeClient = new NamedPipeClientStream(".", "LogPipe", PipeDirection.Out))
            {
                await pipeClient.ConnectAsync();

                using (var writer = new StreamWriter(pipeClient))
                {
                    await writer.WriteAsync(logData);
                    lock(lockObject) orderCount++;
                }
            }
        }

        static string GetRandomStatus()
        {
            string[] statuses = { "접수", "처리중", "완료" };
            return statuses[_random.Next(statuses.Length)];
        }
    }

    public class Order
    {
        public string OrderId { get; set; }
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public int TableNumber { get; set; }
        public int OrderCount { get; set; }
        public DateTime OrderDate { get; set; }
        public string Status { get; set; }
    }
}
