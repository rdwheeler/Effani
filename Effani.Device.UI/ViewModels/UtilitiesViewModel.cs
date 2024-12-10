using DevExpress.CodeParser;
using DevExpress.Mvvm;
using DevExpress.Mvvm.CodeGenerators;
using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Xpf.Core;
using Effani.Device.Control;
using Effani.Device.Process;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Input;

namespace Effani.Device.UI
{
	public partial class UtilitiesViewModel : ViewModelBase, IDisposable
	{
		#region Constructors
		public UtilitiesViewModel(IDeviceService deviceService)
		{
			_device = deviceService.GetDevice();
			_device.DataReceived += Device_DataRecieved;
			_processEngine = deviceService.GetProcessEngine();
			_processEngine.MessageReceived += ProcessEngine_DataReceived;
			Axes = new ObservableCollection<AxisSpec>(_device.Configuration.DeviceSpecification.Axes);
		}
		#endregion

		#region Public methods
		public void Dispose()
		{
			_processEngine.Dispose();
			_processEngine = null;
			_device.Dispose();
			_device = null;
		}
		#endregion

		#region Private methods
		private void AddConsoleMessage(string message)
		{
			Application.Current.Dispatcher
				.Invoke(
					() =>
					{
						while (ConsoleMessages.Count >= 10000)
							ConsoleMessages.RemoveAt(9999);
						ConsoleMessages.Add($"{DateTime.Now.ToShortTimeString()} {message}");
					});
		}
		private void Device_DataRecieved(object sender, Control.DeviceControl.DataArgs<string> e)
		{
			AddConsoleMessage($"{e.Data}");
		}
		private void ProcessEngine_DataReceived(object sender, DeviceControl.DataArgs<string> e)
		{
			AddConsoleMessage($"{e.Data}");
		}
		#endregion

		#region Properties
		public ObservableCollection<AxisSpec> Axes { get; set; }
		public ObservableCollection<string> ConsoleMessages { get; set; } = new();
		#endregion

		#region Fields
		DeviceControl _device;
		ProcessEngine _processEngine = new ProcessEngine();
		#endregion

		#region Commands
		[Command]
		public async Task GenerateConfigFiles()
		{
			try
			{
				var fileLocation = _device.Configuration
					.GenerateConfigFiles(@"I:\DriveTrain\Effani\Effani.Device.Control");
				string messageBoxText = $"Config generated at {fileLocation}";
				string caption = "Generate Config";

				DXMessageBox.Show(messageBoxText, caption);
			}
			catch (Exception ex)
			{
				var a = ex;
			}
		}

		[Command]
		public async Task AlignToFirstWell(AxisSpec axisSpec)
		{
			var plate = _device.Configuration.DeviceSpecification.Plates
				.FirstOrDefault(p => p.Index.X == 1 && p.Index.Y == 0);
			_processEngine.QueueCommand(new AlignWellToAxis(axisSpec.Axis, plate.Wells.First(), _device));
		}

		[Command]
		public async Task ApplyConfig()
		{
			_device.ApplyConfig();
		}

		[Command]
		public async Task Home(AxisSpec axisSpec)
		{
			_processEngine.QueueCommand(new HomeAxis(axisSpec.Axis, _device));
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
			Mouse.OverrideCursor = Cursors.Wait;
			var response = _device.ReadDna();
			AddConsoleMessage($"DNA: {response}");
			Mouse.OverrideCursor = Cursors.Arrow;
		}

		[Command]
		public async Task ReadNadh()
		{
			Mouse.OverrideCursor = Cursors.Wait;
			var response = _device.ReadNadh();
			AddConsoleMessage($"NADH: {response}");
			Mouse.OverrideCursor = Cursors.Arrow;
		}

		[Command]
		public async Task LaserNadhOn()
		{
			var response = _device.LaserNadhOn();
			AddConsoleMessage(response);
		}

		[Command]
		public async Task LaserNadhOff()
		{
			var response = _device.LaserNadhOff();
			AddConsoleMessage(response);
		}

		[Command]
		public async Task LaserDnaOn()
		{
			var response = _device.LaserDnaOn();
			AddConsoleMessage(response);
		}

		[Command]
		public async Task LaserDnaOff()
		{
			var response = _device.LaserDnaOff();
			AddConsoleMessage(response);
		}

		[Command]
		public async Task EnableCommands()
		{
			_device.Configuration.BypassDeviceCommands = 2;
		}
		[Command]
		public async Task DisableCommands()
		{
			_device.Configuration.BypassDeviceCommands = 0;
		}

		[Command]
		public async Task SetWifi()
		{
			try
			{
				var ssid = "blah - 2.4Ghz";// ConfigurationManager.AppSettings["wifi-ssid"];
				var pass = "B4forever!";//ConfigurationManager.AppSettings["wifi-password"];
				var response = _device.SetWifi(ssid, pass);
				AddConsoleMessage(response);
			}
			catch (Exception ex)
			{
				var a = ex;
			}
		}

		[Command]
		public async Task GetBoardDiagnostics(int boardNumber)
		{
			try
			{
				var response = _device.GetBoardDiagnostics(boardNumber);
				AddConsoleMessage(response);
			}
			catch (Exception ex)
			{
				var a = ex;
			}
		}

		[Command]
		public async Task ClearMessages()
		{
			ConsoleMessages.Clear();
		}

		[Command]
		public async Task MoveRapid(List<object> parameters)
		{
			var axisSpec = (AxisSpec)parameters[0];
			var distance = (float)parameters[1];
			var response = _device.Move(axisSpec.Axis, distance);
			AddConsoleMessage(response);
		}
		#endregion
	}
}
