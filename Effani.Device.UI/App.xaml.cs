using DevExpress.Mvvm.POCO;
using DevExpress.Xpf.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using System;
using System.IO;
using System.Windows;

namespace Effani.Device.UI
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application
	{
		static App()
		{
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .WriteTo.File(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Effani", "Log.txt"),
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}", flushToDiskInterval: TimeSpan.FromMilliseconds(500))
                .CreateLogger();

            AppHost = Host.CreateDefaultBuilder()
				.ConfigureServices((hostContext, services) =>
				{
					services.AddSingleton<IDeviceService, DeviceService>();
					services.AddSingleton<MainWindow>();
					services.AddSingleton<MainView>();
					services.AddSingleton<UtilitiesView>();
				})
				.Build();

			ApplicationThemeHelper.ApplicationThemeName = Theme.MetropolisDark.Name;
		}

		protected override async void OnStartup(StartupEventArgs e)
		{
			await AppHost!.StopAsync();

			var startupForm = AppHost.Services.GetRequiredService<MainWindow>();
			startupForm.Show();

			base.OnStartup(e);

			DevExpress.Xpf.Core.ApplicationThemeHelper.UpdateApplicationThemeName();
		}
		protected override async void OnExit(ExitEventArgs e)
		{
			await AppHost!.StopAsync();
            Log.CloseAndFlush();

            base.OnExit(e);
		}

		public static IHost AppHost { get; private set; }
	}
}
