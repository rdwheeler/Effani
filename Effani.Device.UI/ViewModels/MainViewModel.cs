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
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Input;

namespace Effani.Device.UI
{
	public class SampleLoadingProcess(Well well)
	{
		public Well Well { get; } = well;
	}
	public partial class MainViewModel : ViewModelBase, IDisposable
	{

		public MainViewModel()
		{
			_device = new DeviceControl("COM5");
			_device.DataReceived += Device_DataRecieved;
			_processEngine = new ProcessEngine();
			_processEngine.DataReceived += ProcessEngine_DataReceived;
			Axes = new ObservableCollection<AxisSpec>(_device.Configuration.DeviceSpecification.Axes);
		}

		#region Commands
		[Command]
		public async Task CreateAssay()
		{
			if (NumberOfTests == 0 || string.IsNullOrEmpty(AccountName))
				return;

			Assay = new Assay(_device.Configuration, NumberOfTests);
		}
		[Command]
		public async Task GenerateConfigFiles()
		{
			try
			{
				var fileLocation = _device.Configuration.GenerateConfigFiles(@"I:\DriveTrain\Effani\Effani.Device.Control");
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
			_processEngine.AddCommand(new AlignWellToAxis(axisSpec.Axis, plate.Wells.First(), _device));
		}
		[Command]
		public async Task ApplyConfig()
		{
			_device.ApplyConfig();
		}

		[Command]
		public async Task Home(AxisSpec axisSpec)
		{
			_processEngine.AddCommand(new HomeAxis(axisSpec.Axis, _device));
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
		public void PreparationPageLoaded()
		{
			Assay.DoPrepToTimerStart();
		}
		public void LoadingSamplesPageLoaded()
		{
			Assay.LoadSamples();
		}
		public void LoadFluidSystemPageLoaded()
		{
			Assay.LoadSamples();
		}
		public void AssayPageLoaded()
		{
			Assay.LoadSamples();
		}
		public void CleanUpPageLoaded()
		{
			Assay.LoadSamples();
		}
		public void AssayConcludedPageLoaded()
		{
			Assay.LoadSamples();
		}
		public void Dispose()
		{
			_processEngine.Dispose();
			_processEngine = null;
			_device.Dispose();
			_device = null;
		}

		private void AddConsoleMessage(string message)
		{
			Application.Current.Dispatcher.Invoke(() =>
				{
					while(ConsoleMessages.Count >= 10000)
						ConsoleMessages.RemoveAt(9999);
					ConsoleMessages.Add(message);
				});
		}

		private void ProcessEngine_DataReceived(object sender, DeviceControl.DataArgs<string> e)
		{
			AddConsoleMessage($"{e.Data}");
		}

		private void Device_DataRecieved(object sender, Control.DeviceControl.DataArgs<string> e)
		{
			AddConsoleMessage($"{e.Data}");
		}
		private void NumberOfTestsOrAccountNameChanged()
		{
			if(AccountAndNumberOfTestsComplete)
			{
				if(Assay != null)
				{
					Assay.StatusMessages.CollectionChanged -= StatusMessages_CollectionChanged;
					Assay.Dispose();
					Assay = null;
				}
				Assay = new Assay(_device, NumberOfTests, _processEngine);
				Assay.StatusMessages.CollectionChanged += StatusMessages_CollectionChanged;
			}
		}

		private void StatusMessages_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			throw new NotImplementedException();
		}

		public ObservableCollection<AxisSpec> Axes { get; set; }
		public ObservableCollection<SampleLoadingProcess> SampleLoadingProcesses { get; set; }
		int _numberOfTests;
		public int NumberOfTests
		{
			get
			{
				return _numberOfTests;
			}
			set
			{
				if (_numberOfTests == value)
					return;
				_numberOfTests = value;
				RaisePropertyChanged(nameof(NumberOfTests));
				RaisePropertyChanged(nameof(AccountAndNumberOfTestsComplete));
				NumberOfTestsOrAccountNameChanged();
			}
		}
		string _accountName;
		public string AccountName
		{
			get
			{
				return _accountName;
			}
			set
			{
				if (_accountName == value)
					return;
				_accountName = value;
				RaisePropertyChanged(nameof(AccountName));
				RaisePropertyChanged(nameof(AccountAndNumberOfTestsComplete));
				NumberOfTestsOrAccountNameChanged();
			}
		}

		public bool AccountAndNumberOfTestsComplete => !String.IsNullOrEmpty(AccountName) && NumberOfTests != 0;
		public bool PreparationComplete
		{
			get
			{
				return _preparationComplete;
			}
			set
			{
				if (_preparationComplete == value)
					return;
				_preparationComplete = value;
				RaisePropertyChanged(nameof(PreparationComplete));
			}
		}
		public Assay Assay { get; private set; }

		public ObservableCollection<string> ConsoleMessages { get; set; } = new();

		DeviceControl _device;
		ProcessEngine _processEngine = new ProcessEngine();
		bool _preparationComplete;
	}
}
