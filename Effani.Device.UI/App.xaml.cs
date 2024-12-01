using DevExpress.Xpf.Core;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
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
			//CompatibilitySettings.UseLightweightThemes = true;
			ApplicationThemeHelper.ApplicationThemeName = Theme.MetropolisDark.Name;
		}
		private void OnAppStartup_UpdateThemeName(object sender, StartupEventArgs e)
		{
			DevExpress.Xpf.Core.ApplicationThemeHelper.UpdateApplicationThemeName();
		}
	}
}
