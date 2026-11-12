using DevExpress.Xpf.Core;
using Drivetrain.Robotics.Process;
using Effani.Device.Control;
using Effani.Device.Services;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Controls;

namespace Effani.Device.UI
{
	public partial class UtilitiesView : ThemedWindow
	{
		public UtilitiesView(ISettingsService settingsService, ProcessEngine processEngine, EffaniDeviceControl device, ObservableCollection<string> consoleMessages, bool isAssayRunning = false)
		{
			InitializeComponent();
			// settings may have been changed since the device configuration was built: rebuild it from the current settings,
			// unless an assay is running or commands are still queued or executing, which depend on the configuration they started with
			if (!isAssayRunning && !processEngine.IsBusy)
				device.RefreshSettings();
			DataContext = new UtilitiesViewModel(settingsService, processEngine, device, consoleMessages) { IsAssayRunning = isAssayRunning };
			this.Height = (System.Windows.SystemParameters.PrimaryScreenHeight * 0.90);
			this.Width = (System.Windows.SystemParameters.PrimaryScreenWidth * 0.90);
		}
	}
}
