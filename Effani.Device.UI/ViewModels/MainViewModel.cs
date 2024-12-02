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
	public partial class MainViewModel : ViewModelBase, IDisposable
	{

		public MainViewModel()
		{
			_device = new DeviceControl("COM5");
			_device.DataReceived += Device_DataRecieved;
			_processEngine = new ProcessEngine();
			_processEngine.DataReceived += ProcessEngine_DataReceived;
			Axes = new ObservableCollection<AxisSpec>(_device.Configuration.DeviceSpecification.Axes);
			Assay = new Assay(_device, _processEngine);
		}

		#region Commands
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

		[Command]
		public async Task AcceptSample()
		{
			CurrentLoadingSample.TestMediaTimer1h.Start();
			CurrentLoadingSample.TestMediaTimer2h.Start();
			CurrentLoadingSample.TestMediaTimer3h.Start();
			CurrentLoadingSample.TestMediaTimer4h.Start();
			SamplesLoaded.Add(CurrentLoadingSample);
			var currentIndex = SampleLoadingCollection.IndexOf(CurrentLoadingSample);
			CurrentLoadingSample = SampleLoadingCollection.ElementAtOrDefault(currentIndex + 1);
			if(CurrentLoadingSample is not null)
			{
				AddConsoleMessage($"Preparing {CurrentLoadingSample.Well.Label} for sample loading");
				_processEngine.AddCommand(new AlignWellToAxis(Axis.Biopsy, CurrentLoadingSample.Well, _device));
			}
		}
		[Command]
		public async Task LoadSample()
		{
			if(CurrentLoadingSample == null || CurrentLoadingSample.Loaded)
				return;

			AddConsoleMessage($"Injecting sample to {CurrentLoadingSample.Well.Label}");
			_processEngine.AddCommand(new EmitCo2(_device));
			_processEngine.AddCommand(new AlignWellToLocation(_device.Configuration.CameraLocation, CurrentLoadingSample.Well, _device));
			CurrentLoadingSample.Loaded = true;
		}

		[Command]
		public void SkipSample()
		{
			if(CurrentLoadingSample is null)
				return;

			AddConsoleMessage($"Invalidating sample in {CurrentLoadingSample.Well.Label}");
			CurrentLoadingSample.Canceled = true;
		}
		#endregion
		public void LoadFluidSystemPageStart()
		{
			AddConsoleMessage("Prompting to load fluid system");
			_processEngine.AddCommand(new HomeAll(_device));
		}
		public void LoadSampleTraysPageStart()
		{
			AddConsoleMessage("Prompting to load sample trays");
			_processEngine.AddCommand(new HomeAxis(Axis.TableX, _device));
			_processEngine.AddCommand(new HomeAxis(Axis.TableY, _device));
		}
		public void PreparationPageStart()
		{
			Assay.Initialize(NumberOfTests);
			Assay.StatusMessages.CollectionChanged += StatusMessages_CollectionChanged;
			Assay.PreparationComplete += Assay_PreparationComplete;
			Assay.TestInKohPhase += Assay_TestInKohPhase;
			Assay.TestInDnaPhase += Assay_TestInDnaPhase;
			Assay.TestComplete += Assay_TestComplete;
			Assay.Terminated += Assay_Terminated;
			Task.Run(() => Assay.DoPrepToTimerStart());
		}

		private void Assay_TestComplete(object sender, DeviceControl.DataArgs<TestWell> e)
		{
			Application.Current.Dispatcher.Invoke(() =>
			{
				SamplesLoaded.Remove(e.Data);
				SamplesInKoh.Remove(e.Data);
				SamplesInDna.Remove(e.Data);
				SamplesComplete.Add(e.Data);
				if(SamplesLoaded.Count() == 0 && SamplesInKoh.Count() == 0 && SamplesInDna.Count() == 0)
				{
					AddConsoleMessage("Assay complete");
					AssayComplete = true;
				}
			});
		}

		private void Assay_TestInDnaPhase(object sender, DeviceControl.DataArgs<TestWell> e)
		{
			Application.Current.Dispatcher.Invoke(() =>
			{
				SamplesLoaded.Remove(e.Data);
				SamplesInKoh.Remove(e.Data);
				SamplesInDna.Add(e.Data);
			});
		}

		private void Assay_TestInKohPhase(object sender, DeviceControl.DataArgs<TestWell> e)
		{
			Application.Current.Dispatcher.Invoke(() =>
			{
				SamplesLoaded.Remove(e.Data);
				SamplesInKoh.Add(e.Data);
			});
		}

		public void LoadingSamplesPageStart()
		{
			AddConsoleMessage("Prompting to load samples");
			SampleLoadingCollection.AddRange(Assay.TestWells1);
			SampleLoadingCollection.AddRange(Assay.TestWells2);
			CurrentLoadingSample = SampleLoadingCollection.First();
			_processEngine.AddCommand(new AlignWellToAxis(Axis.Biopsy, CurrentLoadingSample.Well, _device));
		}
		public void FirstPageLoaded()
		{
			AddConsoleMessage("Collecting test parameters");
		}
		public void AssayPageStart()
		{
			Assay.DoAssay();
		}
		public void CleanUpPageStart()
		{
			var results = new StringBuilder();
			results.AppendLine(AccountName);
			results.AppendLine($"Low NADH:{Assay.LowNadh}");
			results.AppendLine($"High NADH:{Assay.HighNadh}");
			results.AppendLine($"1 µg DNA:{Assay.Microgram1Dna}");
			results.AppendLine($"0.25 µg DNA:{Assay.MicrogramDna25Dna}");
			results.AppendLine($"0.0625 µg DNA:{Assay.MicrogramDna0625Dna}");
			results.AppendLine($"0 µg DNA:{Assay.Microgram0Dna}");
			foreach (var well in Assay.WellTests)
				results.AppendLine($"{well.SampleId} {well.Label} DNA:{well.DnaValue} {String.Join(' ', well.TimeValues.Select(t => $"T{t.Key}:{t.Value}"))}");
			File.WriteAllText(ResultsFilePath, results.ToString());

			Assay.Dispose();
			Assay = null;
		}
		public void AssayConcludedPageStart()
		{
			AddConsoleMessage("Assay concluded");
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
				ConsoleMessages.Add($"{DateTime.Now.ToShortTimeString()} {message}");
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

		private void Assay_Terminated(object sender, EventArgs e)
		{
		}

		private void Assay_PreparationComplete(object sender, EventArgs e)
		{
			Application.Current.Dispatcher.Invoke(() =>
			{
				PreparationComplete = true;
			});
		}

		private void StatusMessages_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
            foreach (var message in e.NewItems.OfType<string>())
            {
				AddConsoleMessage(message);
            }
		}

		public ObservableCollection<AxisSpec> Axes { get; set; }

		public List<TestWell> SampleLoadingCollection { get; set; } = new();
		public TestWell CurrentLoadingSample
		{
			get
			{
				return _currentLoadingSample;
			}
			set
			{
				if (_currentLoadingSample == value)
					return;
				_currentLoadingSample = value;
				RaisePropertyChanged(nameof(CurrentLoadingSample));
			}
		}
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
				ResultsFilePath = Path.Combine(Environment.CurrentDirectory, $"Results {value} {DateTime.Now.ToString("yyyy-dd-M--HH-mm-ss")}.effani");
				RaisePropertyChanged(nameof(AccountName));
				RaisePropertyChanged(nameof(AccountAndNumberOfTestsComplete));
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
		public bool AssayComplete
		{
			get
			{
				return _assayComplete;
			}
			set
			{
				if (_assayComplete == value)
					return;
				_assayComplete = value;
				RaisePropertyChanged(nameof(AssayComplete));
			}
		}
		public bool AssayTerminated
		{
			get
			{
				return _assayTerminated;
			}
			set
			{
				if (_assayTerminated == value)
					return;
				_assayTerminated = value;
				RaisePropertyChanged(nameof(AssayTerminated));
			}
		}
		public Assay Assay { get; private set; }
		public string ResultsFilePath { get; set; }

		public ObservableCollection<string> ConsoleMessages { get; set; } = new();
		public ObservableCollection<TestWell> SamplesLoaded { get; set; } = new();
		public ObservableCollection<TestWell> SamplesInKoh { get; set; } = new();
		public ObservableCollection<TestWell> SamplesInDna { get; set; } = new();
		public ObservableCollection<TestWell> SamplesComplete { get; set; } = new();

		DeviceControl _device;
		ProcessEngine _processEngine = new ProcessEngine();
		bool _preparationComplete;
		bool _assayComplete;
		TestWell _currentLoadingSample;
		bool _assayTerminated;
	}
}
