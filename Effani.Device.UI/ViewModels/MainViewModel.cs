using DevExpress.CodeParser;
using DevExpress.Mvvm;
using DevExpress.Mvvm.CodeGenerators;
using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Xpf.CodeView;
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
		#region Constructors
		public MainViewModel(IDeviceService deviceService)
		{
			_device = deviceService.GetDevice();
			_device.DataReceived += Device_DataRecieved;
			_processEngine = deviceService.GetProcessEngine();
			_processEngine.DataReceived += ProcessEngine_DataReceived;
			Axes = new ObservableCollection<AxisSpec>(_device.Configuration.DeviceSpecification.Axes);
			Assay = new Assay(_device, _processEngine);
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
		public void PageStartFirstPage()
		{
			AddConsoleMessage("Collecting test parameters");
		}
		public void PageStartLoadFluidSystem()
		{
			Assay.Initialize(NumberOfTests);
			Assay.StatusMessages.CollectionChanged += StatusMessages_CollectionChanged;
			Assay.PreparationComplete += Assay_PreparationComplete;
			Assay.TestInKohPhase += Assay_TestInKohPhase;
			Assay.TestInDnaPhase += Assay_TestInDnaPhase;
			Assay.TestComplete += Assay_TestComplete;
			Assay.Terminated += Assay_Terminated;
			AddConsoleMessage("Prompting to load fluid system");
			_processEngine.QueueCommand(new TurnOnTrayHeat(_device));
			_processEngine.QueueCommand(new HomeAll(_device));
		}
		public void PageStartLoadSampleTrays()
		{
			AddConsoleMessage("Prompting to load sample trays");
		}
		public void PageStartLoadingSamples()
		{
			AddConsoleMessage("Prompting to load samples");
            //_processEngine.QueueCommands(new CommandPacket([
            //    new AlignWellToAxis(Axis.Biopsy, plate.Wells.First(), _device),
            //    new AxisGoto(Axis.Biopsy, _device.Configuration.BiopsyTravel, _device),
            //    new AxisGoto(Axis.BiopsyEject, _device.Configuration.BiopsyZTravel, _device),
            //    new EmitCo2(_device),
            //    new AxisGoto(Axis.BiopsyEject, _device.Configuration.BiopsyEjectHomePosition, _device),
            //    new AxisGoto(Axis.Biopsy, _device.Configuration.BiospsyLoadingLocation, _device),
            //    new AlignWellToLocation(_device.Configuration.CameraLocation,  well, _device)
            //]));
            SampleLoadingCollection.AddRange(Assay.TestWells1);
			SampleLoadingCollection.AddRange(Assay.TestWells2);
			CurrentLoadingSample = SampleLoadingCollection.First();
		}
		public void PageStartAssay()
		{
			Assay.DoAssay();
		}
		public void PageStartAssayConcluded()
		{
			AddConsoleMessage("Assay concluded");
		}
		public void PageStartCleanUp()
		{
			var results = new StringBuilder();
			_processEngine.QueueCommand(new TurnOffTrayHeat(_device));
			results.AppendLine(AccountName);
			results.AppendLine($"Low NADH:{Assay.LowNadh}");
			results.AppendLine($"High NADH:{Assay.HighNadh}");
			results.AppendLine($"1 µg DNA:{Assay.Microgram1Dna}");
			results.AppendLine($"0.25 µg DNA:{Assay.MicrogramDna25Dna}");
			results.AppendLine($"0.0625 µg DNA:{Assay.MicrogramDna0625Dna}");
			results.AppendLine($"0 µg DNA:{Assay.Microgram0Dna}");
			foreach (var well in Assay.WellTests)
				results.AppendLine(
					$"{well.SampleId} {well.Label} DNA:{well.DnaValue} {String.Join(' ', well.TimeValues.Select(t => $"T{t.Key}:{t.Value}"))}");
			File.WriteAllText(ResultsFilePath, results.ToString());

			Assay.Dispose();
			Assay = null;
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
						ConsoleMessages.Insert(0, $"{DateTime.Now.ToShortTimeString()} {message}");
					});
		}
		private void Assay_PreparationComplete(object sender, EventArgs e)
		{
			Application.Current.Dispatcher
				.Invoke(
					() =>
					{
						PreparationComplete = true;
					});
		}
		private void Assay_Terminated(object sender, EventArgs e)
		{
			_processEngine.QueueCommand(new TurnOffTrayHeat(_device));
		}
		private void Assay_TestComplete(object sender, DeviceControl.DataArgs<TestWell> e)
		{
			Application.Current.Dispatcher
				.Invoke(
					() =>
					{
						SamplesLoaded.Remove(e.Data);
						SamplesInKoh.Remove(e.Data);
						SamplesInDna.Remove(e.Data);
						SamplesComplete.Add(e.Data);
						if (SamplesLoaded.Count() == 0 && SamplesInKoh.Count() == 0 && SamplesInDna.Count() == 0)
						{
							AddConsoleMessage("Assay complete");
							AssayComplete = true;
							AdvanceWizardPage?.Invoke(this, null);
						}
					});
		}
		private void Assay_TestInDnaPhase(object sender, DeviceControl.DataArgs<TestWell> e)
		{
			Application.Current.Dispatcher
				.Invoke(
					() =>
					{
						SamplesLoaded.Remove(e.Data);
						SamplesInKoh.Remove(e.Data);
						SamplesInDna.Add(e.Data);
					});
		}
		private void Assay_TestInKohPhase(object sender, DeviceControl.DataArgs<TestWell> e)
		{
			Application.Current.Dispatcher
				.Invoke(
					() =>
					{
						SamplesLoaded.Remove(e.Data);
						SamplesInKoh.Add(e.Data);
					});
		}
		private void Device_DataRecieved(object sender, Control.DeviceControl.DataArgs<string> e)
		{
			AddConsoleMessage($"{e.Data}");
		}
		private void PrepareNextSampleForLoading()
		{
			var currentIndex = SampleLoadingCollection.IndexOf(CurrentLoadingSample);
			CurrentLoadingSample = SampleLoadingCollection.ElementAtOrDefault(currentIndex + 1);
			if(CurrentLoadingSample == null)
				AdvanceWizardPage?.Invoke(this, null);
		}
		private void ProcessEngine_DataReceived(object sender, DeviceControl.DataArgs<string> e)
		{
			AddConsoleMessage($"{e.Data}");
		}
		private void StatusMessages_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			foreach (var message in e.NewItems.OfType<string>())
			{
				AddConsoleMessage(message);
			}
		}
        int emitFluidVolume;
        #endregion

        #region Properties
        public int EmitFluidVolume
        {
            get => emitFluidVolume;
            set
            {
                if (emitFluidVolume == value)
                {
                    return;
                }

                emitFluidVolume = value;
                RaisePropertyChanged(nameof(EmitFluidVolume));
            }
        }
        public bool AccountAndNumberOfTestsComplete => !String.IsNullOrEmpty(AccountName) && NumberOfTests != 0;
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
				ResultsFilePath = Path.Combine(
					Environment.CurrentDirectory,
					$"Results {value} {DateTime.Now.ToString("yyyy-dd-M--HH-mm-ss")}.effani");
				RaisePropertyChanged(nameof(AccountName));
				RaisePropertyChanged(nameof(AccountAndNumberOfTestsComplete));
			}
		}
		public Assay Assay { get; private set; }
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
		public ObservableCollection<AxisSpec> Axes { get; set; }
		public ObservableCollection<string> ConsoleMessages { get; set; } = new();
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
				UnloadedTestsCount = value;
			}
		}
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
		public string ResultsFilePath { get; set; }
		public List<TestWell> SampleLoadingCollection { get; set; } = new();
		public ObservableCollection<TestWell> SamplesComplete { get; set; } = new();
		public ObservableCollection<TestWell> SamplesInDna { get; set; } = new();
		public ObservableCollection<TestWell> SamplesInKoh { get; set; } = new();
		public ObservableCollection<TestWell> SamplesLoaded { get; set; } = new();
		public int UnloadedTestsCount
		{
			get
			{
				return _unloadedTestsCount;
			}
			set
			{
				if (_unloadedTestsCount == value)
					return;
				_unloadedTestsCount = value;
				RaisePropertyChanged(nameof(UnloadedTestsCount));
			}
		}

		public EventHandler AdvanceWizardPage;
		#endregion

		#region Fields
		string _accountName;
		bool _assayComplete;
		bool _assayTerminated;
		TestWell _currentLoadingSample;
		DeviceControl _device;
		int _numberOfTests;
		bool _preparationComplete;
		int _unloadedTestsCount;
		ProcessEngine _processEngine;
		#endregion

		#region Commands
		[Command]
		public void AcceptSample()
		{
			_processEngine.ProcessingLock.ExitWriteLock();
			CurrentLoadingSample.TestMediaTimer1h.Start();
			CurrentLoadingSample.TestMediaTimer2h.Start();
			CurrentLoadingSample.TestMediaTimer3h.Start();
			CurrentLoadingSample.TestMediaTimer4h.Start();
			SamplesLoaded.Add(CurrentLoadingSample);
			UnloadedTestsCount--;
			PrepareNextSampleForLoading();
		}
		[Command]
		public void FluidLoaded()
        {
            //AddConsoleMessage($"Priming fluid pumps");
            //_processEngine.ProcessingLock.EnterWriteLock();
            //var packet = new CommandPacket();
            //packet.Commands.AddRange([new AlignTableForWaste(_device)]);
            //foreach (var pump in _device.FluidAxes)
            //{
            //    packet.Commands.AddRange([new PrimeFluidPump(pump.Axis, _device.Configuration.FluidPrimingVolume, _device)]);
            //}
            //_processEngine.QueueCommands(packet);

            AdvanceWizardPage?.Invoke(this, new EventArgs());
		}
		[Command]
		public void InjectSample()
		{
			AddConsoleMessage($"Injecting sample to {CurrentLoadingSample.Well.Label}");
			_processEngine.ProcessingLock.EnterWriteLock();
			_processEngine.QueueCommands(new CommandPacket([
				new AlignWellToAxis(Axis.Biopsy, CurrentLoadingSample.Well, _device),
                new AxisGoto(Axis.Biopsy, _device.Configuration.BiospsyLoadingLocation + _device.Configuration.BiopsyTravel, _device),
                new AxisGoto(Axis.BiopsyEject, _device.Configuration.BiopsyEjectHomePosition + _device.Configuration.BiopsyZTravel, _device),
                new EmitCo2(_device),
                new AxisGoto(Axis.BiopsyEject, _device.Configuration.BiopsyEjectHomePosition, _device),
                new AxisGoto(Axis.Biopsy, _device.Configuration.BiospsyLoadingLocation, _device),
                new AlignWellToLocation(_device.Configuration.CameraLocation, CurrentLoadingSample.Well, _device)
			]));
			CurrentLoadingSample.LoadedOntoTray = true;
		}
		[Command]
		public void RetrySample()
		{
			AddConsoleMessage($"Redoing sample in {CurrentLoadingSample.Well.Label}");
			_processEngine.QueueCommand(new AlignWellToLocation(_device.Configuration.CameraLocation, CurrentLoadingSample.Well, _device));
			CurrentLoadingSample.LoadedOntoTray = false;
		}
		[Command]
		public void SkipSample()
		{
			_processEngine.ProcessingLock.ExitWriteLock();
			AddConsoleMessage($"Skipping sample in {CurrentLoadingSample.Well.Label}");
			CurrentLoadingSample.Canceled = true;
			PrepareNextSampleForLoading();
		}
		[Command]
		public void SkipRemaining()
		{
			_processEngine.ProcessingLock.ExitWriteLock();
			if (CurrentLoadingSample is not null)
				AddConsoleMessage($"Skipping samples after number {CurrentLoadingSample.TestIndex}, {CurrentLoadingSample.SampleId}, {CurrentLoadingSample.Label}");
			CurrentLoadingSample = null; // triggers the loading process is complete
			AdvanceWizardPage?.Invoke(this, new EventArgs());
		}
		[Command]
		public void TraysLoaded()
		{
			new System.Threading.Thread(() => Assay.DoPrepToTimerStart()).Start();
			AdvanceWizardPage?.Invoke(this, new EventArgs());
		}
		[Command]
		public async Task GenerateConfigFiles()
		{
			try
			{
				var fileLocation = _device.Configuration
					.GenerateConfigFiles("C:");
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
            var plate = _device.Configuration.DeviceSpecification.Plates.First();
            var well = plate.Wells.First();
            _processEngine.QueueCommand(new AlignWellToAxis(axisSpec.Axis, well, _device));
        }

        [Command]
        public async Task AlignToLastWell(AxisSpec axisSpec)
        {
            var plate = _device.Configuration.DeviceSpecification.Plates.First();
            var well = plate.Wells.Last();
            _processEngine.QueueCommand(new AlignWellToAxis(axisSpec.Axis, well, _device));
        }

        [Command]
        public async Task EmitFluid(AxisSpec axisSpec)
        {
            Mouse.OverrideCursor = Cursors.Wait;
            var plate = _device.Configuration.DeviceSpecification.Plates
                .FirstOrDefault(p => p.Index.X == 0 && p.Index.Y == 0);
            _processEngine.QueueCommand(new EmitFluid(axisSpec.Axis, EmitFluidVolume, _device));
            Mouse.OverrideCursor = Cursors.Arrow;
        }

        [Command]
        public async Task HomeAll()
        {
            Mouse.OverrideCursor = Cursors.Wait;
            _processEngine.QueueCommand(new HomeAll(_device));
            Mouse.OverrideCursor = Cursors.Arrow;
        }

        [Command]
        public async Task AlignForWaste()
        {
            Mouse.OverrideCursor = Cursors.Wait;
            _processEngine.QueueCommand(new AlignTableForWaste(_device));
            Mouse.OverrideCursor = Cursors.Arrow;
        }

        [Command]
        public async Task InjectSampleTest()
        {
            Mouse.OverrideCursor = Cursors.Wait;
            var plate = _device.Configuration.DeviceSpecification.Plates.First();
            var well = plate.Wells.First();
            _processEngine.QueueCommands(new CommandPacket([
                new AxisGoto(Axis.BiopsyEject, _device.Configuration.BiopsyEjectHomePosition, _device),
                new AxisGoto(Axis.Biopsy, _device.Configuration.BiospsyLoadingLocation, _device),
                new AlignWellToAxis(Axis.Biopsy, plate.Wells.First(), _device),
                new AxisGoto(Axis.Biopsy, _device.Configuration.BiospsyLoadingLocation + _device.Configuration.BiopsyTravel, _device),
                new AxisGoto(Axis.BiopsyEject, _device.Configuration.BiopsyEjectHomePosition + _device.Configuration.BiopsyZTravel, _device),
                new EmitCo2(_device),
                new AxisGoto(Axis.BiopsyEject, _device.Configuration.BiopsyEjectHomePosition, _device),
                new AxisGoto(Axis.Biopsy, _device.Configuration.BiospsyLoadingLocation, _device),
                new AlignWellToLocation(_device.Configuration.CameraLocation,  well, _device)
            ]));
            Mouse.OverrideCursor = Cursors.Arrow;
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
        public async Task AlignToNadhLaser()
        {
            var plate = _device.Configuration.DeviceSpecification.Plates.First();
            var well = plate.Wells.First();
            _processEngine.QueueCommand(new AlignWellToLocation(_device.Configuration.NadhLaserLocation, well, _device));
        }

        [Command]
        public async Task AlignToDnaLaser()
        {
            var plate = _device.Configuration.DeviceSpecification.Plates.First();
            var well = plate.Wells.First();
            _processEngine.QueueCommand(new AlignWellToLocation(_device.Configuration.DnaLaserLocation, well, _device));
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
