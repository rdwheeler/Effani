using DevExpress.Mvvm.POCO;
using DevExpress.Xpf.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
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

			base.OnExit(e);
		}

		public static IHost AppHost { get; private set; }
	}
}
