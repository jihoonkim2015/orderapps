using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;

namespace pos.wpf.winapp
{
    public class MainWindowViewModel : ObservableObject
    {
        private ObservableCollection<Order>[] _tableOrders;

        public MainWindowViewModel()
        {
            int tableCount = (int)Application.Current.Resources["TableCount"];
            _tableOrders = new ObservableCollection<Order>[tableCount];
            for (int i = 0; i < tableCount; i++)
            {
                _tableOrders[i] = new ObservableCollection<Order>();
            }

            ChangeOrderStatusCommand = new AsyncRelayCommand<Order>(async order => await ChangeOrderStatus(order));
            StartServer();
            StartLogServer();
        }

        public IAsyncRelayCommand<Order> ChangeOrderStatusCommand { get; }

        public ObservableCollection<Order> GetOrdersForTable(int tableNumber)
        {
            return _tableOrders[tableNumber - 1];
        }

        private async void StartServer()
        {
            while (true)
            {
                using (var pipeServer = new NamedPipeServerStream("OrderPipe", PipeDirection.In))
                {
                    await Task.Run(() => pipeServer.WaitForConnection());

                    using (var reader = new StreamReader(pipeServer))
                    {
                        string jsonOrder = await reader.ReadToEndAsync();
                        var order = JsonSerializer.Deserialize<Order>(jsonOrder);
                        UpdateOrderStatus(order);
                    }
                }
            }
        }

        private async void StartLogServer()
        {
            while (true)
            {
                using (var pipeServer = new NamedPipeServerStream("LogPipe", PipeDirection.In))
                {
                    await Task.Run(() => pipeServer.WaitForConnection());

                    using (var reader = new StreamReader(pipeServer))
                    {
                        string logData = await reader.ReadToEndAsync();
                        await SendLogDataToWorker(logData);
                    }
                }
            }
        }


        private void UpdateOrderStatus(Order newOrder)
        {
            if (newOrder.TableNumber > 0 && newOrder.TableNumber <= _tableOrders.Length)
            {
                var orders = _tableOrders[newOrder.TableNumber - 1];
                var existingOrder = orders.FirstOrDefault();
                if (existingOrder == null)
                {
                    orders.Add(newOrder);
                }
                else if(existingOrder.Status.Equals("완료"))
                {
                    existingOrder.Status = newOrder.Status;
                }
            }
        }

        private async Task ChangeOrderStatus(Order order)
        {
            if (order != null)
            {
                await SendOrderStatusUpdate(order);
            }
        }

        private async Task SendOrderStatusUpdate(Order order)
        {
            using (var pipeClient = new NamedPipeClientStream(".", "ProcOrderPipe", PipeDirection.Out))
            {
                await pipeClient.ConnectAsync();

                using (var writer = new StreamWriter(pipeClient))
                {
                    var jsonOrder = JsonSerializer.Serialize(order);
                    await writer.WriteAsync(jsonOrder);
                }
            }
        }

        private async Task SendLogDataToWorker(string logData)
        {
            using (var pipeClient = new NamedPipeClientStream(".", "ProcLogPipe", PipeDirection.Out))
            {
                await pipeClient.ConnectAsync();

                using (var writer = new StreamWriter(pipeClient))
                {
                    await writer.WriteAsync(logData);
                }
            }
        }

    }
}
