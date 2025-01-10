using System.Windows;
using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace pos.wpf.winapp
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            Ioc.Default.ConfigureServices(
                new ServiceCollection()
                    .AddSingleton<MainWindowViewModel>()
                    .BuildServiceProvider()
            );

            base.OnStartup(e);
        }
    }
}
