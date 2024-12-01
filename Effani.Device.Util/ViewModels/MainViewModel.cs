using DevExpress.CodeParser;
using DevExpress.Mvvm;
using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Xpf.Docking;
using Effani.Device.Control;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace Effani.Device.Util.ViewModels
{
	public class MainViewModel : ViewModelBase
	{
		public MainViewModel()
		{
			_spec = new DeviceConfiguration().GetDeviceSpecification();
			Axes = new ObservableCollection<AxisSpec>(_spec.Axes);
			_device = new Effani.Device.Control.Device("COM5");
			_device.DataReceived += _device_DataRecieved;
		}
		private void _device_DataRecieved(object sender, Control.Device.DataArgs<string> e)
		{
			AddConsoleMessage($"{e.Data}");
		}

		[Command]
		public async Task GenerateConfigFiles()
		{
			try
			{
				var config = new DeviceConfiguration();
				var fileLocation = config.GenerateConfigFiles(@"I:\DriveTrain\Effani\Effani.Device.Control"); 
				string messageBoxText = $"Config generated at {fileLocation}";
				string caption = "Generate Config";

				MessageBox.Show(messageBoxText, caption);
			}
			catch (Exception ex)
			{
				var a = ex;
			}
		}
		[Command]
		public async Task Home(AxisSpec axis)
		{
			var response = _device.Home(axis);
			AddConsoleMessage(response);
		}
		[Command]
		public async Task PlateUvOn()
		{
			var response = _device.PlateUvOn();
			AddConsoleMessage(response);
		}
		[Command]
		public async Task PlateUvOff()
		{
			var response = _device.PlateUvOff();
			AddConsoleMessage(response);
		}
		[Command]
		public async Task BiopsyUvOn()
		{
			var response = _device.BiopsyUvOn();
			AddConsoleMessage(response);
		}
		[Command]
		public async Task BiopsyUvOff()
		{
			var response = _device.BiopsyUvOff();
			AddConsoleMessage(response);
		}
		[Command]
		public async Task ReadDna()
		{
			var response = _device.ReadDna();
			AddConsoleMessage(response);
		}
		[Command]
		public async Task ReadNadh()
		{
			var response = _device.ReadNadh();
			AddConsoleMessage(response);
		}
		[Command]
		public async Task SetWifi()
		{
			var ssid = "blah - 2.4Ghz";// ConfigurationManager.AppSettings["wifi-ssid"];
			var pass = "B4forever!";//ConfigurationManager.AppSettings["wifi-password"];
			var response = _device.SetWifi(ssid, pass);
			AddConsoleMessage(response);
		}
		[Command]
		public async Task GetBoardDiagnostics()
		{
			var response = _device.GetBoardDiagnostics(2);
			AddConsoleMessage(response);
		}
		[Command]
		public async Task MoveRapid(List<object> parameters)
		{
			var axis = (AxisSpec)parameters[0];
			var distance = (float)parameters[1];
			var response = _device.Move(axis, distance);
			AddConsoleMessage(response);
		}

		private void AddConsoleMessage(string message)
		{
			while(ConsoleMessages.Count >= 10)
				ConsoleMessages.RemoveAt(9);
			ConsoleMessages.Add(message);
		}

		public ObservableCollection<AxisSpec> Axes { get; set; }

		public ObservableCollection<string> ConsoleMessages { get; set; } = new();

		DeviceSpecification _spec;
		Effani.Device.Control.Device _device;
	}
}
