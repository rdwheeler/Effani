using DevExpress.Xpf.Core;
using DevExpress.Xpf.Editors;
using Drivetrain.Robotics.Control;
using Drivetrain.Robotics.Process;
using Effani.Device.Control;
using Effani.Device.Services;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Controls;

namespace Effani.Device.UI
{
	public partial class SettingsControl : UserControl
	{
		public SettingsControl()
		{
			InitializeComponent();
		}

		private void ComboBoxEdit_SelectedIndexChanged(object sender, System.Windows.RoutedEventArgs e)
		{
			var settingsVersion = ((ComboBoxEdit)e.Source).SelectedItem as SettingsVersion;
			_viewModel.ActivateSettingsVersion(settingsVersion);

		}

		UtilitiesViewModel _viewModel => DataContext as UtilitiesViewModel;
    }
}
