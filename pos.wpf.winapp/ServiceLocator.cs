using Microsoft.Extensions.DependencyInjection;
using System;

namespace pos.wpf.winapp
{
    public class ServiceLocator
    {
        public IServiceProvider ServiceProvider { get; }

        public ServiceLocator()
        {
            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);
            ServiceProvider = serviceCollection.BuildServiceProvider();
        }

        private void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<MainWindowViewModel>();
        }

        public MainWindowViewModel MainWindowViewModel => ServiceProvider.GetRequiredService<MainWindowViewModel>();
    }
}
