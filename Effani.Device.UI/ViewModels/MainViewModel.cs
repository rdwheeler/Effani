using DevExpress.CodeParser;
using DevExpress.Mvvm;
using DevExpress.Mvvm.CodeGenerators;
using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Xpf.CodeView;
using DevExpress.Xpf.Core;
using DevExpress.XtraScheduler.Commands;
using Effani.Device.Control;
using Effani.Device.Process;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Configuration;
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
			_processEngine.MessageReceived += ProcessEngine_MessageReceived;
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
        }
		public void PageStartLoadFluidSystem()
		{
			Assay.Initialize(NumberOfTests);
			Assay.StatusMessages.CollectionChanged += StatusMessages_CollectionChanged;
			Assay.PreparationComplete += Assay_PreparationComplete;
			Assay.TestInNadhReadingsPhase += Assay_TestInNadhPhase;
			Assay.TestInDnaPhase += Assay_TestInDnaPhase;
			Assay.TestInKohPhase += Assay_TestInKohPhase;
			Assay.TestComplete += Assay_TestComplete;
			Assay.Terminated += Assay_Terminated;
            if (DoHome)
            {
                _processEngine.QueueCommand(new SetTableTemp(_config.TrayTemperature, _device));
				_processEngine.QueueCommand(new UvBiopsyOff(_device));
				_processEngine.QueueJob(new Job("Moving all controls to loading positions.", [
					new HomeAll(_device),
					new MoveToStartingLocations(_device),
                    new TableToTrayLoadingLocation(_device)
				]));
            }
		}
		public void PageStartLoadSampleTrays()
		{
        }
        public void PageStartLoadingSamples()
		{
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
			_processEngine.QueueCommand(new SetTableTemp(0, _device));
			results.AppendLine(AccountName);
			foreach (var well in Assay.TestWells)
				results.AppendLine(
					$"{well.SampleId},{well.Label},Low NADH:{well.LowNadh},High NADH:{well.HighNadh},1 µg DNA:{well.Microgram1Dna},0.25 µg DNA:{well.MicrogramDna25Dna},0.0625 µg DNA:{well.MicrogramDna0625Dna},0 µg DNA:{well.Microgram0Dna},DNA:{well.DnaValue},{String.Join(',', well.TimeValues.Select(t => $"T{t.Key}:{t.Value}"))}");
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
			_processEngine.QueueCommand(new SetTableTemp(0, _device));
		}
		private void Assay_TestComplete(object sender, DataArgs<TestWell> e)
		{
			Application.Current.Dispatcher
				.Invoke(
					() =>
					{
						SamplesInNadh.Remove(e.Data);
						SamplesInKoh.Remove(e.Data);
						SamplesInDna.Remove(e.Data);
						SamplesComplete.Add(e.Data);
						if (SamplesInNadh.Count() == 0 && SamplesInKoh.Count() == 0 && SamplesInDna.Count() == 0)
						{
							AddConsoleMessage("Assay complete");
							AssayComplete = true;
							//AdvanceWizardPage?.Invoke(this, null);
						}
					});
		}
		private void Assay_TestInNadhPhase(object sender, DataArgs<TestWell> e)
		{
			Application.Current.Dispatcher
				.Invoke(
					() =>
					{
						SamplesInBasal.Remove(e.Data);
						SamplesInNadh.Add(e.Data);
					});
		}
		private void Assay_TestInKohPhase(object sender, DataArgs<TestWell> e)
		{
			Application.Current.Dispatcher
				.Invoke(
					() =>
					{
						SamplesInNadh.Remove(e.Data);
						SamplesInKoh.Add(e.Data);
					});
		}
		private void Assay_TestInDnaPhase(object sender, DataArgs<TestWell> e)
		{
			Application.Current.Dispatcher
				.Invoke(
					() =>
					{
						SamplesInNadh.Remove(e.Data);
						SamplesInKoh.Remove(e.Data);
						SamplesInDna.Add(e.Data);
					});
		}
		private void Device_DataRecieved(object sender, DataArgs<string> e)
		{
			AddConsoleMessage($"{e.Data}");
			File.AppendAllLines(LogFilePath, new string[] { $"{GetShortNow()} {e.Data}" });
		}
		private void PrepareNextSampleForLoading()
		{
			var currentIndex = SampleLoadingCollection.IndexOf(CurrentLoadingSample);
			CurrentLoadingSample = SampleLoadingCollection.ElementAtOrDefault(currentIndex + 1);
            if (CurrentLoadingSample == null)
            {
                //AdvanceWizardPage?.Invoke(this, null);
            }
		}
		private void ProcessEngine_MessageReceived(object sender, DataArgs<string> e)
		{
			AddConsoleMessage($"{e.Data}");
			File.AppendAllLines(LogFilePath, new string[] { $"{GetShortNow()} {e.Data}" });
		}
		private void StatusMessages_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			foreach (var message in e.NewItems.OfType<string>())
			{
				AddConsoleMessage(message);
			}
		}
		private string GetShortNow()
		{
			return $"{DateTime.Now.ToString("yyyy-dd-M--HH-mm-ss")}";
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
				ResultsFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    $"Results {value} {GetShortNow()}.effani");
				LogFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
					$"Log {value} {GetShortNow()}.txt");
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
		public string LogFilePath { get; set; }
		public List<TestWell> SampleLoadingCollection { get; set; } = new();
		public ObservableCollection<TestWell> SamplesComplete { get; set; } = new();
		public ObservableCollection<TestWell> SamplesInDna { get; set; } = new();
		public ObservableCollection<TestWell> SamplesInKoh { get; set; } = new();
		public ObservableCollection<TestWell> SamplesInNadh { get; set; } = new();
		public ObservableCollection<TestWell> SamplesInBasal { get; set; } = new();
		public Well CurrentWell
		{
			get
			{
				if (_currentWell == null && _device is not null)
					_currentWell = _device.Configuration.DeviceSpecification.Plates.FirstOrDefault()?.Wells.FirstOrDefault();
				return _currentWell;
			}
			set
			{
				_currentWell = value;
				RaisePropertyChanged(nameof(CurrentWell));
			}
		}
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
		AbalDeviceControl _device;
		DeviceConfiguration _config => _device.Configuration;
		int _numberOfTests;
		bool _preparationComplete;
		int _unloadedTestsCount;
		ProcessEngine _processEngine;

        private bool DoHome = false;
        private bool DoPrime = false;
        private bool DoPrep = false;
        public static bool LoadStandardFluids = false;
        public static bool DoNadhReadings = true;
        public static bool DoTestMediaTimerSteps = true;
        public static bool DoKohTimerSteps = true;
        public static bool DoDnaTimerSteps = false;
        private Well _currentWell;
		#endregion

		#region Commands
		[Command]
		public void AcceptSample()
        {
            _processEngine.HoldQueueForHighPriorityJob = false;

			SamplesInBasal.Add(CurrentLoadingSample);
			UnloadedTestsCount--;
			PrepareNextSampleForLoading();
		}
		[Command]
		public void FluidLoaded()
        {
            if (DoPrime)
            {
                var job = new Job("Priming fluid pumps.");
                foreach (var axis in _device.FluidAxes)
                {
					job.Commands.Add(new EmitFluid(axis.Axis, _device.Configuration.FluidPrimingVolume, _device));
                }
                _processEngine.QueueJob(job);
            }

            //AdvanceWizardPage?.Invoke(this, new EventArgs());
		}
		[Command]
		public void InjectSample()
		{
            _processEngine.HoldQueueForHighPriorityJob = true;

			var basalAxis = CurrentLoadingSample.Well.Plate.Index.X == 0 ? Axis.Basal1 : Axis.Basal2;
			_processEngine.QueuePriorityJob(new Job($"Adding Basal media to {CurrentLoadingSample.Label}.", [
				new AlignWellToAxis(basalAxis, CurrentLoadingSample.Well, _device),
				new EmitFluid(basalAxis, _device.Configuration.BasalVolume, _device)
			]));
			_processEngine.QueuePriorityJob(new Job($"Aligning {CurrentLoadingSample.Label} for biopsy sample loading.", [
				new AlignWellToLocation(_device.Configuration.CameraLocation, CurrentLoadingSample.Well, _device)
			]));
			CurrentLoadingSample.LoadedOntoTray = true;
		}
		[Command]
		public void SkipSample()
        {
            _processEngine.HoldQueueForHighPriorityJob = false;

            AddConsoleMessage($"Skipping sample in {CurrentLoadingSample.Well.Label}");
			CurrentLoadingSample.Canceled = true;
			PrepareNextSampleForLoading();
		}
		[Command]
		public void SkipRemaining()
        {
            _processEngine.HoldQueueForHighPriorityJob = false;

            if (CurrentLoadingSample is not null)
				AddConsoleMessage($"Skipping samples after number {CurrentLoadingSample.TestIndex}, {CurrentLoadingSample.SampleId}, {CurrentLoadingSample.Label}");
			CurrentLoadingSample = null; // triggers the loading process is complete
			AdvanceWizardPage?.Invoke(this, new EventArgs());
		}
		[Command]
		public void TraysLoaded()
		{
            if (DoPrep)
			    new System.Threading.Thread(() => Assay.ProcessStandards()).Start();
			//AdvanceWizardPage?.Invoke(this, new EventArgs());
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
        public async Task AlignPrime(AxisSpec axisSpec)
        {
            if (axisSpec is FluidAxis)
            {
                _processEngine.QueueJob(new Job([
                    new HomeAxis(axisSpec.Axis, _device),
                    new MoveAxisAbsolute(axisSpec.Axis, _device.Configuration.FluidSyringeHomePosition, _device),
                ]));
            }
            if (axisSpec is AspirateAxis)
            {
                _processEngine.QueueJob(new Job([
                    new HomeAxis(axisSpec.Axis, _device),
                    new MoveAxisAbsolute(axisSpec.Axis, _device.Configuration.AspirateKohSyringeHomePosition, _device),
                ]));
            }
        }
        [Command]
		public async Task AlignToFirstWell(AxisSpec axisSpec)
        {
            var plate = _device.Configuration.DeviceSpecification.Plates.First();
            var well = plate.Wells.First();
            CurrentWell = well;
            _processEngine.QueueCommand(new AlignWellToAxis(axisSpec.Axis, well, _device));
        }
        [Command]
        public async Task AlignToNextWell(AxisSpec axisSpec)
        {
            var plate = _device.Configuration.DeviceSpecification.Plates.First();
            if (CurrentWell == null)
                CurrentWell = plate.Wells.First();
            var nextIndex = plate.Wells.IndexOf(CurrentWell) + 1;
            if (plate.Wells.Count - 1 < nextIndex)
                return;
            var well = plate.Wells[nextIndex];
            CurrentWell = well;
            _processEngine.QueueCommand(new AlignWellToAxis(axisSpec.Axis, well, _device));
        }

        [Command]
        public async Task AlignToLastWell(AxisSpec axisSpec)
        {
            var plate = _device.Configuration.DeviceSpecification.Plates.First();
            var well = plate.Wells.Last();
            CurrentWell = well;
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
        public async Task EmptyKoh(AxisSpec axisSpec)
        {
            Mouse.OverrideCursor = Cursors.Wait;
            _processEngine.QueueCommand(new EmptyAspirateKoh(_device));
            Mouse.OverrideCursor = Cursors.Arrow;
        }

        [Command]
        public async Task AspirateKoh(AxisSpec axisSpec)
        {
            Mouse.OverrideCursor = Cursors.Wait;
            _processEngine.QueueCommand(new RemoveKoh(EmitFluidVolume, _device));
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
		public async Task MoveToStartingLocations()
		{
			Mouse.OverrideCursor = Cursors.Wait;
			_processEngine.QueueCommand(new MoveToStartingLocations(_device));
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
			var well = CurrentWell;
            _processEngine.QueueJob(new Job([
                new MoveAxisAbsolute(Axis.BiopsyEject, _device.Configuration.BiopsyEjectHomePosition, _device),
                new MoveAxisAbsolute(Axis.Biopsy, _device.Configuration.BiospsyLoadingLocation, _device),
                new AlignWellToAxis(Axis.Biopsy, well, _device),
                new UvBiopsyOn(_device),
                new MoveAxisAbsolute(Axis.Biopsy, _device.Configuration.BiospsyLoadingLocation + _device.Configuration.BiopsyTravel, _device),
                new UvBiopsyOff(_device),
                new MoveAxisAbsolute(Axis.BiopsyEject, _device.Configuration.BiopsyEjectHomePosition + _device.Configuration.BiopsyZTravel, _device),
                new EmitCo2(_device),
                new MoveAxisAbsolute(Axis.BiopsyEject, _device.Configuration.BiopsyEjectHomePosition, _device),
                new MoveAxisAbsolute(Axis.Biopsy, _device.Configuration.BiospsyLoadingLocation, _device),
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
        public void PrimeFluid()
        {
            AddConsoleMessage($"Priming fluid pumps");
            var job = new Job();
            foreach (var axis in _device.FluidAxes)
            {
                packet.Commands.Add(new EmitFluid(axis.Axis, _device.Configuration.FluidPrimingVolume, _device));
            }
            _processEngine.QueueJob(packet);
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
				var ssid = ConfigurationManager.AppSettings["wifi-ssid"];
				var pass = ConfigurationManager.AppSettings["wifi-password"];
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
			var response = _device.MoveRelative(axisSpec.Axis, distance);
			AddConsoleMessage(response);
		}
		#endregion
    }
}
