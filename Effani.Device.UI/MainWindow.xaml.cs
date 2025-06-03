using DevExpress.Xpf.Core;
using Effani.Device.DataCollection;
using Effani.Device.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Effani.Device.UI
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : ThemedWindow
	{
		public MainWindow(IDeviceService deviceService, IAssayManagerService assayManagerService, ISettingsService settingsService)
		{
			InitializeComponent();
			DataContext = new MainViewModel(deviceService, assayManagerService, settingsService);
		}
		protected override void OnClosing(CancelEventArgs e)
		{
			(DataContext as MainViewModel).Dispose();
			base.OnClosing(e);
		}
	}
}
