using System.Windows;
using System.Windows.Controls;
using CommunityToolkit.Mvvm.DependencyInjection;
using System.IO.Pipes;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Data;

namespace pos.wpf.winapp
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            int tableCount = (int)Application.Current.Resources["TableCount"];
            DataContext = Ioc.Default.GetRequiredService<MainWindowViewModel>();
            CreateTableUI(tableCount);
        }

        private void CreateTableUI(int tableCount)
        {
            for (int i = 0; i < tableCount; i++)
            {
                var groupBox = new GroupBox
                {
                    Header = $"Table {i + 1}",
                    Margin = new Thickness(10)
                };

                var listBox = new ListBox
                {
                    ItemsSource = ((MainWindowViewModel)DataContext).GetOrdersForTable(i + 1)
                };

                var dataTemplate = new DataTemplate(typeof(Order));
                var stackPanel = new FrameworkElementFactory(typeof(StackPanel));

                var productNameBlock = new FrameworkElementFactory(typeof(TextBlock));
                productNameBlock.SetBinding(TextBlock.TextProperty, new System.Windows.Data.Binding("ProductName"));
                stackPanel.AppendChild(productNameBlock);

                var statusBlock = new FrameworkElementFactory(typeof(TextBlock));
                statusBlock.SetBinding(TextBlock.TextProperty, new System.Windows.Data.Binding("Status"));
                stackPanel.AppendChild(statusBlock);

                var comboBox = new FrameworkElementFactory(typeof(ComboBox));
                comboBox.SetValue(ComboBox.ItemsSourceProperty, new[] { "접수", "처리중", "완료" });
                comboBox.SetBinding(ComboBox.SelectedItemProperty, new System.Windows.Data.Binding("Status") { UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged });
                comboBox.AddHandler(ComboBox.SelectionChangedEvent, new SelectionChangedEventHandler(StatusComboBox_SelectionChanged));
                stackPanel.AppendChild(comboBox);

                dataTemplate.VisualTree = stackPanel;
                listBox.ItemTemplate = dataTemplate;

                groupBox.Content = listBox;
                MainGrid.Children.Add(groupBox);
                Grid.SetRow(groupBox, i / 2);
                Grid.SetColumn(groupBox, i % 2);
            }
        }

        private async void StatusComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0 && ((ComboBox)sender).DataContext is Order order)
            {
                await ((MainWindowViewModel)DataContext).ChangeOrderStatusCommand.ExecuteAsync(order);
            }
        }
    }
}
