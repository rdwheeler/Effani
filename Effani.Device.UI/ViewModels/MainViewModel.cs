using DevExpress.CodeParser;
using DevExpress.Mvvm;
using DevExpress.Mvvm.CodeGenerators;
using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Xpf.CodeView;
using DevExpress.Xpf.Core;
using DevExpress.XtraScheduler.Commands;
using Drivetrain.Robotics.Control;
using Drivetrain.Robotics.Process;
using Serilog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
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
			_processEngine.BeginExecutingCommand += ProcessEngine_BeginExecutingCommand;
			_processEngine.EndExecutingCommand += ProcessEngine_EndExecutingCommand;
			Axes = new ObservableCollection<AxisSpec>(_device.Configuration.DeviceSpecification.Axes);
			Assay = new Assay(_device, _processEngine);
		}
		#endregion

		#region Public methods
		public void Dispose()
		{
			_device.DataReceived -= Device_DataRecieved;
			_processEngine.MessageReceived -= ProcessEngine_MessageReceived;
			_processEngine.BeginExecutingCommand -= ProcessEngine_BeginExecutingCommand;
			_processEngine.EndExecutingCommand -= ProcessEngine_EndExecutingCommand;
			_processEngine.Dispose();
			_processEngine = null;
			_device.Dispose();
			_device = null;
		}
		public void CancelAssay()
		{
			SamplesInBasal.Clear();
			SamplesInNadh.Clear();
			SamplesInKoh.Clear();
			SamplesInDna.Clear();
			NumberOfTests = SamplesComplete.Count();
			UnloadedTestsCount = 0;
			AddMessage("Assay canceled", true);
			AssayComplete = true;
		}
		public void PageStartFirstPage()
		{
        }
		public void PageStartLoadFluidSystem()
		{
			Assay.Initialize(NumberOfTests);
			Assay.StatusMessages.CollectionChanged += StatusMessages_CollectionChanged;
			Assay.StandardsComplete += Assay_StandardsComplete;
			Assay.TestInNadhReadingsPhase += Assay_TestInNadhPhase;
			Assay.TestInDnaPhase += Assay_TestInDnaPhase;
			Assay.TestInKohPhase += Assay_TestInKohPhase;
			Assay.TestComplete += Assay_TestComplete;
			Assay.Terminated += Assay_Terminated;
            if (DoHome)
            {
                //_processEngine.QueueCommand(new SetTableTemp(_config.TrayTemperature, _device));
				_processEngine.QueueJob(new Job("Moving all controls to loading positions.", [
					new HomeAll(_device),
					new MoveToStartingLocations(_device),
                    new TableToTrayLoadingLocation(_device),
					new NotifyComplete((command) => { NotifyMoveStartingPositionsComplete(); }, "Notify move to starting positions complete.", _device)
				]));
			}
			else
			{
				_processEngine.QueueCommand(new NotifyComplete((command) => { NotifyMoveStartingPositionsComplete(); }, "Notify move to starting positions complete.", _device));
			}
		}
		public void PageStartLoadSampleTrays()
		{
			Thread.Sleep(1000);        
		}
        public void PageStartLoadingSamples()
		{
            SampleLoadingCollection.AddRange(Assay.TestWells1);
			SampleLoadingCollection.AddRange(Assay.TestWells2);
			CurrentLoadingSample = SampleLoadingCollection.First();
		}
		public void PageStartAssay()
		{

		}
		public void PageStartAssayConcluded()
		{
			AddMessage("Assay concluded", true);
		}
		public void PageStartCleanUp()
		{
			var results = new StringBuilder();
			//_processEngine.QueueCommand(new SetTableTemp(0, _device));
			_processEngine.QueueCommand(new MoveToStartingLocations(_device));
			_processEngine.QueueCommand(new TableToTrayLoadingLocation(_device));
			results.AppendLine(AccountName);
			results.AppendLine($"Low NADH:{Assay.LowNadh}");
			results.AppendLine($"High NADH:{Assay.HighNadh}");
			results.AppendLine($"1 µg DNA:{Assay.Microgram1Dna}");
			results.AppendLine($"0.25 µg DNA:{Assay.MicrogramDna25Dna}");
			results.AppendLine($"0.0625 µg DNA:{Assay.MicrogramDna0625Dna}");
			results.AppendLine($"0 µg DNA:{Assay.Microgram0Dna}");
			foreach (var well in Assay.TestWells.Skip(_config.StandardsWellCount).Where(w => !w.Canceled))
				results.AppendLine(
					$"{well.SampleId},{well.Label},DNA:{well.DnaValue},{String.Join(',', well.TimeValues.Select(t => $"T{t.Key}:{t.Value}"))}");
			File.WriteAllText(ResultsFilePath, results.ToString());

			Assay.Dispose();
			Assay = null;
		}
		#endregion

		#region Private methods
		private void AddMessage(string message, bool addToProcessMessages)
		{
			Application.Current.Dispatcher
				.Invoke(
					() =>
					{
						var now = DateTime.Now.ToString("H:mm:ss");
						while (ConsoleMessages.Count >= 10000)
							ConsoleMessages.RemoveAt(9999);
						ConsoleMessages.Insert(0, $"{now} {message}");
						if (addToProcessMessages)
						{
							while (ProcessMessages.Count >= 10000)
								ProcessMessages.RemoveAt(9999);
							ProcessMessages.Insert(0, $"{now} {message}");
						}
					});
		}
		private void NotifyMoveStartingPositionsComplete()
		{
			Application.Current.Dispatcher
				.Invoke(
					() =>
					{
						MoveStartingPositionsComplete = true;
					});
		}
		private void NotifyFluidPrimingComplete()
		{
			Application.Current.Dispatcher
				.Invoke(
					() =>
					{
						Assay.FluidPrimingComplete = true;
					});
		}
		private void Assay_StandardsComplete(object sender, EventArgs e)
		{
			Application.Current.Dispatcher
				.Invoke(
					() =>
					{
						StandardsComplete = true;
					});
		}
		private void Assay_Terminated(object sender, EventArgs e)
		{
			CancelAssay();
		}
		private void Assay_TestComplete(object sender, DataArgs<TestWell> e)
		{
			Application.Current.Dispatcher
				.Invoke(
					() =>
					{
						if(e.Data is not null)
						{
							SamplesInDna.Remove(e.Data);
							SamplesComplete.Add(e.Data);
						}
						if (SamplesComplete.Count() == NumberOfTests)
						{
							AddMessage("Assay complete", true);
							AssayComplete = true;
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
						SamplesInKoh.Remove(e.Data);
						SamplesInDna.Add(e.Data);
					});
		}
		private void Device_DataRecieved(object sender, DataArgs<string> e)
		{
			AddMessage($"{e.Data}", false);
		}
		private void PrepareNextSampleForLoading()
		{
			PreparedToLoadSample = false;
			var currentIndex = SampleLoadingCollection.IndexOf(CurrentLoadingSample);
			CurrentLoadingSample = SampleLoadingCollection.ElementAtOrDefault(currentIndex + 1);
            if (CurrentLoadingSample == null)
            {
                AdvanceWizardPage?.Invoke(this, null);
            }
		}
		private void ProcessEngine_MessageReceived(object sender, DataArgs<string> e)
		{
			AddMessage($"{e.Data}", true);
			Log.Information(e.Data);
		}

		private void ProcessEngine_EndExecutingCommand(object sender, DataArgs<Command> e)
		{
			ExecutingCommandText = "None";
		}

		private void ProcessEngine_BeginExecutingCommand(object sender, DataArgs<Command> e)
		{
			ExecutingCommandText = e.Data.Description;
		}
		private void StatusMessages_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			foreach (var message in e.NewItems.OfType<string>())
			{
				AddMessage(message, true);
			}
		}
		private void NotifyPreparedToLoadSample()
		{
			Application.Current.Dispatcher
				.Invoke(
					() =>
					{
						PreparingToLoadSample = false;
						PreparedToLoadSample = true;
					});
		}
		private string GetShortNow()
		{
			return $"{DateTime.Now.ToString("yyyy-dd-M--HH-mm-ss")}";
		}
		#endregion

		#region Properties
		public int EmitFluidVolume
        {
            get => _emitFluidVolume;
            set
            {
                if (_emitFluidVolume == value)
                {
                    return;
                }

                _emitFluidVolume = value;
                RaisePropertyChanged(nameof(EmitFluidVolume));
            }
		}
		public string SendCommandText
		{
			get => _sendCommandText;
			set
			{
				if (_sendCommandText == value)
				{
					return;
				}

				_sendCommandText = value;
				RaisePropertyChanged(nameof(SendCommandText));
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
				ResultsFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                    $"Results {value} {GetShortNow()}.effani");
				LogFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
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
		public ObservableCollection<string> ProcessMessages { get; set; } = new();
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
		public string ExecutingCommandText
		{
			get
			{
				return _executingCommandText;
			}
			set
			{
				if (_executingCommandText == value)
					return;
				_executingCommandText = value;
				RaisePropertyChanged(nameof(ExecutingCommandText));
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
		public bool StandardsComplete
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
				RaisePropertyChanged(nameof(StandardsComplete));
			}
		}
		public bool SampleLoadingInProcess
		{
			get
			{
				return PreparingToLoadSample;
			}
			set
			{
				if (PreparingToLoadSample == value)
					return;
				PreparingToLoadSample = value;
				RaisePropertyChanged(nameof(SampleLoadingInProcess));
			}
		}
		public bool MoveStartingPositionsComplete
		{
			get
			{
				return _moveStartingPositionsComplete;
			}
			set
			{
				if (_moveStartingPositionsComplete == value)
					return;
				_moveStartingPositionsComplete = value;
				RaisePropertyChanged(nameof(MoveStartingPositionsComplete));
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
		public bool PreparingToLoadSample
		{
			get
			{
				return _preparingToLoadSample;
			}
			set
			{
				if (_preparingToLoadSample == value)
					return;
				_preparingToLoadSample = value;
				RaisePropertyChanged(nameof(PreparingToLoadSample));
			}
		}
		#region PreparedToLoadSample
		public bool PreparedToLoadSample
		{
			get
			{
				return _preparedToLoadSample;
			}
			set
			{
				if (_preparedToLoadSample == value)
					return;
				_preparedToLoadSample = value;
				RaisePropertyChanged(nameof(PreparedToLoadSample));
			}
		}
		bool _preparedToLoadSample;
		#endregion

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
		int _emitFluidVolume;
		string _sendCommandText;
		bool _moveStartingPositionsComplete;
		bool _fluidPrimingComplete;
		string _executingCommandText = "None";

		public static bool DoHome = true;
		public static bool DoPrime = true;
		public static bool DoStandardsSteps = true;
        public static bool DispenseStandardFluids = true;
		public static bool DoSamplePreparationSteps = true;
        public static bool DoNadhReadingSteps = true;
        public static bool DoKohTimerSteps = true;
        public static bool DoDnaIncubationTimerSteps = true;
        private Well _currentWell;
		private bool _preparingToLoadSample;
		#endregion

		#region Commands
		[Command]
		public void AcceptSample()
		{
			var job = new Job($"{CurrentLoadingSample.Well.Label} sample loaded.");
			job.Commands.Add(new StartTimer(CurrentLoadingSample.BasalIncubationTimer, $"{CurrentLoadingSample.Well.Label} basal incubation.", _device));
			_processEngine.QueuePriorityJob(job);

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
				job.Commands.Add(new NotifyComplete((command) => { NotifyFluidPrimingComplete(); }, "Notify fluid priming complete.", _device));
				_processEngine.QueueJob(job);
            }
			else
			{
				_processEngine.QueueCommand(new NotifyComplete((command) => { NotifyFluidPrimingComplete(); }, "Notify fluid priming complete.", _device));
			}

            AdvanceWizardPage?.Invoke(this, new EventArgs());
		}
		[Command]
		public void InjectSample()
		{
            _processEngine.HoldQueueForHighPriorityJob = true;
			PreparingToLoadSample = true;

			if(DoSamplePreparationSteps)
			{
				var basalAxis = CurrentLoadingSample.Well.Plate.Location.X == 0 ? Axis.Basal1 : Axis.Basal2;
				_processEngine.QueuePriorityJob(new Job($"Adding Basal media to {CurrentLoadingSample.Label}.", [
					new AlignWellToAxis(basalAxis, CurrentLoadingSample.Well, _device),
					new EmitFluid(basalAxis, _device.Configuration.BasalVolume, _device)
				]));
				_processEngine.QueuePriorityJob(new Job($"Aligning {CurrentLoadingSample.Label} for biopsy sample loading.", [
					new AlignWellToLocation(_device.Configuration.BiopsyLocation, CurrentLoadingSample.Well, _device),
					new NotifyComplete((command) => { NotifyPreparedToLoadSample(); }, "Notify ready to load sample.", _device)
				]));
			}
			else
			{
				_processEngine.QueuePriorityJob(new Job($"Skipping aligning {CurrentLoadingSample.Label} for biopsy sample loading.", [
					new NotifyComplete((command) => { NotifyPreparedToLoadSample(); }, "Notify ready to load sample.", _device)
				]));
			}
		}
		[Command]
		public void SkipSample()
        {
            _processEngine.HoldQueueForHighPriorityJob = false;

            AddMessage($"Skipping sample in {CurrentLoadingSample.Well.Label}", true);
			CurrentLoadingSample.Canceled = true;
			SamplesComplete.Add(CurrentLoadingSample);
			PrepareNextSampleForLoading();
		}
		[Command]
		public void SkipRemaining()
        {
            _processEngine.HoldQueueForHighPriorityJob = false;

			if(CurrentLoadingSample is not null)
			{
				AddMessage(
					$"Skipping samples after number {CurrentLoadingSample.TestIndex}, {CurrentLoadingSample.SampleId}, {CurrentLoadingSample.Label}", true);
				var currentIndex = Assay.AllTestWells.IndexOf(CurrentLoadingSample);
				foreach(var test in Assay.AllTestWells.Skip(currentIndex))
					test.Canceled = true;
				UnloadedTestsCount = 0;
				NumberOfTests = currentIndex - 1;
			}
			CurrentLoadingSample = null; // triggers the loading process is complete
			PreparedToLoadSample = false;
			UnloadedTestsCount = 0;
			if (SamplesComplete.Count() == NumberOfTests)
			{
				AddMessage("Assay complete", true);
				AssayComplete = true;
			}
			AdvanceWizardPage?.Invoke(this, new EventArgs());
		}
		[Command]
		public void TraysLoaded()
		{
			new System.Threading.Thread(() => Assay.EstablishStandards()).Start();
			AdvanceWizardPage?.Invoke(this, new EventArgs());
		}
		[Command]
		public async Task GenerateConfigFiles()
		{
			try
			{
				var fileLocation = _device.Configuration
					.GenerateConfigFiles(Environment.GetFolderPath(Environment.SpecialFolder.Desktop));
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
        public async Task MoveToStart(AxisSpec axisSpec)
        {
            _processEngine.QueueCommand( new MoveToStartingLocation(axisSpec.Axis, _device));
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
                .FirstOrDefault(p => p.Location.X == 0 && p.Location.Y == 0);
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
			var basalAxis = CurrentLoadingSample.Well.Plate.Location.X == 0 ? Axis.Basal1 : Axis.Basal2;
			_processEngine.QueuePriorityJob(new Job($"Adding Basal media to {CurrentLoadingSample.Label}.", [
				new AlignWellToAxis(basalAxis, CurrentLoadingSample.Well, _device),
				new EmitFluid(basalAxis, _device.Configuration.BasalVolume, _device)
			]));
			_processEngine.QueuePriorityJob(new Job($"Aligning {CurrentLoadingSample.Label} for biopsy sample loading.", [
				new AlignWellToLocation(_device.Configuration.BiopsyLocation, CurrentLoadingSample.Well, _device)
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
			_processEngine.QueueCommand(new Home(_device, axisSpec));
		}

		[Command]
		public async Task TraySterilizerOn()
		{
			_processEngine.QueueCommand(new TraySterilizerOn(_device));
		}

		[Command]
		public async Task TraySterilizerOff()
		{
			_processEngine.QueueCommand(new TraySterilizerOn(_device));
		}

		[Command]
		public async Task ReadDna()
		{
			Mouse.OverrideCursor = Cursors.Wait;
			_processEngine.QueueCommand(new ReadDna(_device, null));
			Mouse.OverrideCursor = Cursors.Arrow;
		}

		[Command]
		public async Task ReadNadh()
		{
			Mouse.OverrideCursor = Cursors.Wait;
			_processEngine.QueueCommand(new ReadNadh(_device, null));
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
			var job = new Job("Priming fluid pumps.");
			foreach (var axis in _device.FluidAxes)
			{
				job.Commands.Add(new EmitFluid(axis.Axis, _device.Configuration.FluidPrimingVolume, _device));
			}
			_processEngine.QueueJob(job);
		}
        [Command]
		public async Task LaserNadhOn()
		{
			_device.LaserNadhOn();
		}
		[Command]
		public async Task LaserNadhOff()
		{
			_device.LaserNadhOff();
		}
		[Command]
		public async Task LaserDnaOn()
		{
			_device.LaserDnaOn();
		}
		[Command]
		public async Task LaserDnaOff()
		{
			_device.LaserDnaOff();
		}
		[Command]
		public async Task EnableCommands()
		{
			_device.Configuration.BypassDeviceCommands = 2000;
		}
		[Command]
		public async Task DisableCommands()
		{
			_device.Configuration.BypassDeviceCommands = 0;
		}
		[Command]
		public async Task SetWifi()
		{
			var ssid = ConfigurationManager.AppSettings["wifi-ssid"];
			var pass = ConfigurationManager.AppSettings["wifi-password"];
			_device.SetWifi(ssid, pass);
		}

		[Command]
		public async Task GetBoardDiagnostics(int boardNumber)
		{
			_processEngine.QueueCommand(new GetBoardDiagnostics(boardNumber, _device));
		}

		[Command]
		public async Task Send()
		{
			_processEngine.QueueCommand(new SendCommand(_sendCommandText, _device));
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
			_processEngine.QueueCommand(new MoveRelative(distance, _device, axisSpec));
		}
		#endregion
    }
}
