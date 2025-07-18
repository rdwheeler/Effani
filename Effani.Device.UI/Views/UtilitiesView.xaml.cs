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
		public UtilitiesView(ISettingsService settingsService, ProcessEngine processEngine, AbalDeviceControl device, ObservableCollection<string> consoleMessages)
		{
			InitializeComponent();
			DataContext = new UtilitiesViewModel(settingsService, processEngine, device, consoleMessages);
			this.Height = (System.Windows.SystemParameters.PrimaryScreenHeight * 0.90);
			this.Width = (System.Windows.SystemParameters.PrimaryScreenWidth * 0.90);
		}
	}
}
