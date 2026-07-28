using DevExpress.Mvvm;
using DevExpress.Xpf.CodeView;
using Drivetrain.Robotics.Control;
using Drivetrain.Robotics.Process;
using Effani.Device.DataCollection;
using Effani.Device.Control;
using Serilog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using Drivetrain;
using DevExpress.Utils.Extensions;
using Effani.Device.Services;
using DevExpress.Xpf.Core;

namespace Effani.Device.UI
{
	public partial class MainViewModel : ViewModelBase, IDisposable
	{
		#region Constructors
		public MainViewModel(IDeviceService deviceService, IAssayManagerService assayManagerService, ISettingsService settingsService)
		{
			_settingsService = settingsService;
			_assayManagerService = assayManagerService;
			_device = deviceService.GetDevice();
			_device.DataReceived += Device_DataRecieved;
			_processEngine = deviceService.GetProcessEngine();
			_processEngine.MessageReceived += ProcessEngine_MessageReceived;
			_processEngine.BeginExecutingCommand += ProcessEngine_BeginExecutingCommand;
			_processEngine.EndExecutingCommand += ProcessEngine_EndExecutingCommand;
			Assay = new DataCollection.Assay(_device, _processEngine);
		}
		#endregion

		#region Public methods
		public void Dispose()
		{
			foreach (var t in SamplesComplete)
				t.Dispose();
			foreach (var t in SamplesInDna)
				t.Dispose();
			foreach (var t in SamplesInKoh)
				t.Dispose();
			foreach (var t in SamplesInNadh)
				t.Dispose();
			foreach (var t in SamplesInBasal)
				t.Dispose();

			if(Assay != null)
			{
				Assay.StatusMessages.CollectionChanged -= StatusMessages_CollectionChanged;
				Assay.StandardsComplete -= Assay_StandardsComplete;
                Assay.TimerMessageChanged -= Assay_TimerMessageChanged;
                Assay.TestInNadhReadingsPhase -= Assay_TestInNadhPhase;
				Assay.TestInDnaPhase -= Assay_TestInDnaPhase;
				Assay.TestInKohPhase -= Assay_TestInKohPhase;
				Assay.TestComplete -= Assay_TestComplete;
				Assay.Terminated -= Assay_Terminated;
			}
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
			NumberOfSamples = SamplesComplete.Count();
			UnloadedTestsCount = 0;
			AddMessage("Assay canceled", true);
			AssayComplete = true;
			AdvanceWizardPage?.Invoke(this, new EventArgs());
		}
		public bool ConfirmClosing()
		{
			string messageBoxText = $"Are you sure you want to cancel this assay and close the program?";
			string caption = "Confirm Closing";

			return DXMessageBox.Show(messageBoxText, caption, MessageBoxButton.YesNo) == MessageBoxResult.Yes;
		}
		public void PageStartLoadFluidSystem()
		{
			Assay.Initialize(NumberOfSamples);
			Assay.StatusMessages.CollectionChanged += StatusMessages_CollectionChanged;
			Assay.StandardsComplete += Assay_StandardsComplete;
            Assay.TimerMessageChanged += Assay_TimerMessageChanged;
            Assay.TestInNadhReadingsPhase += Assay_TestInNadhPhase;
			Assay.TestInDnaPhase += Assay_TestInDnaPhase;
			Assay.TestInKohPhase += Assay_TestInKohPhase;
			Assay.TestComplete += Assay_TestComplete;
			Assay.Terminated += Assay_Terminated;

            //_processEngine.QueueCommand(new SetTableTemp(_config.TrayTemperature, _device));
			_processEngine.QueueJob(new Job("Moving all controls to loading positions.", [
				new HomeAxis(Axis.TableX, _device),
				new HomeAxis(Axis.TableY, _device),
				new HomeAxis(Axis.Koh, _device),
				new HomeAxis(Axis.Basal, _device),
				new HomeAxis(Axis.DnaReagent, _device),
				new HomeAxis(Axis.Nadh, _device),
				new HomeAxis(Axis.AspirateKoh, _device),
				//new HomeAxis(Axis.AspirateFluid, _device),
				new MoveToStartingLocations(_device),
                new TableToTrayLoadingLocation(_device),
				new NotifyComplete((command) => { NotifyMoveStartingPositionsComplete(); }, "Notify move to starting positions complete.", _device)
			]));
		}
        public void PageStartLoadingSamples()
		{
            SampleLoadingCollection.AddRange(Assay.TestWells1);
			SampleLoadingCollection.AddRange(Assay.TestWells2);
			CurrentLoadingSample = SampleLoadingCollection.First();
		}
		public void PageStartAssayConcluded()
        {
            AddMessage("Assay concluded", true);
			AssayFullName = _assayManagerService.SaveAssay(Assay).Result;

			Assay.StatusMessages.CollectionChanged -= StatusMessages_CollectionChanged;
			Assay.StandardsComplete -= Assay_StandardsComplete;
			Assay.StandardsComplete -= Assay_StandardsComplete;
            Assay.TestInNadhReadingsPhase -= Assay_TestInNadhPhase;
			Assay.TestInDnaPhase -= Assay_TestInDnaPhase;
			Assay.TestInKohPhase -= Assay_TestInKohPhase;
			Assay.TestComplete -= Assay_TestComplete;
			Assay.Terminated -= Assay_Terminated;
			Assay.Dispose();
			Assay = null;
		}
		public void PageStartCleanUp()
		{
            AllSamplesLoadedAndProcessing = false;
            //_processEngine.QueueCommand(new SetTableTemp(0, _device));
            //_processEngine.QueueCommand(new MoveToStartingLocations(_device));
            _processEngine.QueueCommand(new TableToTrayLoadingLocation(_device));
		}
        public void InjectSample()
        {
            _processEngine.HoldQueueForHighPriorityJob = true;
            PreparingToLoadSample = true;

            if (!Assay.IsPrimedBasal)
            {
                var job = new Job("Priming basal media.");
                job.Commands.Add(new AlignTableForWaste(_device));
                job.Commands.Add(new EmitFluid(Axis.Basal, _config.Settings.SecondaryBasalFluidPrimingVolume, _device));
                Assay.IsPrimedBasal = true;
                _processEngine.QueuePriorityJob(job);
            }
            _processEngine.QueuePriorityJob(new Job($"Dispensing {_config.Settings.BasalVolume}µg basal media to {CurrentLoadingSample.Label}.", [
				new ReprimeIfNeeded(Axis.Basal, _device),
				new AlignWellToAxis(Axis.Basal, CurrentLoadingSample.Well, _device),
                new EmitFluid(Axis.Basal, _config.Settings.BasalVolume, _device)
            ]));
            _processEngine.QueuePriorityJob(new Job($"Aligning {CurrentLoadingSample.Label} for biopsy sample loading.", [
                new AlignWellToLocation(_config.Settings.BiopsyLocation, CurrentLoadingSample.Well, _device),
                new NotifyComplete((command) => { NotifyPreparedToLoadSample(); }, "Notify ready to load sample.", _device)
            ]));
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
						Assay.InitialFluidPrimingComplete = true;
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
        private void Assay_TimerMessageChanged(object sender, DataArgs<string> e)
        {
            Application.Current.Dispatcher
                .Invoke(
                    () =>
                    {
						if (String.IsNullOrEmpty(e.Data))
						{
							TimerStatusTitle = null;
							TimerStatusMessage = null;
							return;
                        }
                        var split = e.Data.Split("&");
						TimerStatusTitle = split.First();
                        TimerStatusMessage = split.Last();
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
							var vm = SamplesInDna.First(t => t.TestWell == e.Data);
							SamplesInDna.Remove(vm);
							SamplesComplete.Add(vm);
						}
						if (SamplesComplete.Count() == NumberOfSamples)
						{
							AddMessage("Assay complete", true);
							AssayComplete = true;
							AdvanceWizardPage?.Invoke(this, new EventArgs());
						}
					});
		}
		private void Assay_TestInNadhPhase(object sender, DataArgs<TestWell> e)
		{
			Application.Current.Dispatcher
				.Invoke(
					() =>
					{
						var vm = SamplesInBasal.First(t => t.TestWell == e.Data);
						SamplesInBasal.Remove(vm);
						SamplesInNadh.Add(vm);
					});
		}
		private void Assay_TestInKohPhase(object sender, DataArgs<TestWell> e)
		{
			Application.Current.Dispatcher
				.Invoke(
					() =>
					{
						var vm = SamplesInNadh.First(t => t.TestWell == e.Data);
						SamplesInNadh.Remove(vm);
						SamplesInKoh.Add(vm);
					});
		}
		private void Assay_TestInDnaPhase(object sender, DataArgs<TestWell> e)
		{
			Application.Current.Dispatcher
				.Invoke(
					() =>
					{
						var vm = SamplesInKoh.First(t => t.TestWell == e.Data);
						SamplesInKoh.Remove(vm);
						SamplesInDna.Add(vm);
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
                AllSamplesLoadedAndProcessing = true;
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
			if (_config.Settings.PauseCommandsForPrompt)
				MessageBox.Show(e.Data.Description, "Preparing to excute command");

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
		#endregion

		#region Properties
		protected IDialogService DialogService { get { return GetService<IDialogService>(); } }
		public string TimerStatusMessage
        {
            get
            {
                return _sterilizingStatusMessage;
            }
            private set
            {
                if (_sterilizingStatusMessage == value)
                    return;
                _sterilizingStatusMessage = value;
                RaisePropertyChanged(nameof(TimerStatusMessage));
            }
        }
        public string TimerStatusTitle
        {
            get
            {
                return _timerStatusTitle;
            }
            set
            {
                if (_timerStatusTitle == value)
                    return;
                _timerStatusTitle = value;
                RaisePropertyChanged(nameof(TimerStatusTitle));
            }
        }
        public string AssayFullName
		{
			get
			{
				return _assayFullName;
			}
			set
			{
				if (_assayFullName == value)
					return;
				_assayFullName = value;
				RaisePropertyChanged(nameof(AssayFullName));
			}
		}
		public bool HerdIdAndNumberOfTestsComplete => !String.IsNullOrEmpty(HerdId) && NumberOfSamples != 0;
		public bool UsernameAndPasswordComplete => !String.IsNullOrEmpty(Username) && !String.IsNullOrEmpty(Password);
		public string HerdId
		{
			get
			{
				return Assay?.HerdId;
			}
			set
			{
				if(Assay == null)
					return;
				if (Assay.HerdId == value)
					return;
				Assay.HerdId = value;
				RaisePropertyChanged(nameof(HerdId));
				RaisePropertyChanged(nameof(HerdIdAndNumberOfTestsComplete));
			}
		}
		public DataCollection.Assay Assay { get; private set; }
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
		public int NumberOfSamples
		{
			get
			{
				return Assay.NumberOfSamples;
			}
			set
			{
				if (Assay.NumberOfSamples == value)
					return;
				Assay.NumberOfSamples = value;
				RaisePropertyChanged(nameof(NumberOfSamples));
				RaisePropertyChanged(nameof(HerdIdAndNumberOfTestsComplete));
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
		public bool FluidLoadingComplete
		{
			get
			{
				return _fluidLoadingComplete;
			}
			set
			{
				if (_fluidLoadingComplete == value)
					return;
				_fluidLoadingComplete = value;
				RaisePropertyChanged(nameof(FluidLoadingComplete));
			}
		}
		public List<TestWell> SampleLoadingCollection { get; set; } = new();
		public ExplicitBindingList<TestWellViewModel> SamplesComplete { get; set; } = new();
		public ExplicitBindingList<TestWellViewModel> SamplesInDna { get; set; } = new();
		public ExplicitBindingList<TestWellViewModel> SamplesInKoh { get; set; } = new();
		public ExplicitBindingList<TestWellViewModel> SamplesInNadh { get; set; } = new();
		public ExplicitBindingList<TestWellViewModel> SamplesInBasal { get; set; } = new();
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
        public bool AllSamplesLoadedAndProcessing
        {
            get
            {
                return _allSamplesLoaded;
            }
            set
            {
                if (_allSamplesLoaded == value)
                    return;
                _allSamplesLoaded = value;
                RaisePropertyChanged(nameof(AllSamplesLoadedAndProcessing));
            }
        }
        public string Uploading { get; }
		public string Username
		{
			get
			{
				return _settingsService.Settings.EffaniUsername;
			}
			set
			{
				if (_settingsService.Settings.EffaniUsername == value)
					return;
				_settingsService.Settings.EffaniUsername = value;
				RaisePropertyChanged(nameof(Username));
				RaisePropertyChanged(nameof(UsernameAndPasswordComplete));
			}
		}
		public string Password
		{
			get
			{
				return _settingsService.Settings.EffaniPassword;
			}
			set
			{
				if (_settingsService.Settings.EffaniPassword == value)
					return;
				_settingsService.Settings.EffaniPassword = value;
				RaisePropertyChanged(nameof(Password));
				RaisePropertyChanged(nameof(UsernameAndPasswordComplete));
			}
		}
		public bool PreparingToLoadSample
		{
			get
			{
				return _preparingToLoadSample && !String.IsNullOrEmpty(HerdId);
			}
			set
			{
				if (_preparingToLoadSample == value)
					return;
				_preparingToLoadSample = value;
				RaisePropertyChanged(nameof(PreparingToLoadSample));
			}
		}
		public BindingList<string> AssayManagerActivity => _assayManagerService.Activity;
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
		public Uri BackgroundImage { get; set; } = new Uri("../Resources/effani_background.jpg", UriKind.Relative);
		#endregion

		public EventHandler AdvanceWizardPage;

        #region Fields
        string _assayFullName;
        string _sterilizingStatusMessage;
        string _timerStatusTitle;
        bool _assayComplete;
		bool _assayTerminated;
		TestWell _currentLoadingSample;
		AbalDeviceControl _device;
		DeviceConfiguration _config => _device.Configuration;

		bool _preparationComplete;
		int _unloadedTestsCount;
        bool _allSamplesLoaded;
		ProcessEngine _processEngine;
		bool _moveStartingPositionsComplete;
		bool _fluidLoadingComplete;
		string _executingCommandText = "None";
		bool _preparingToLoadSample;
		readonly IAssayManagerService _assayManagerService;
		readonly ISettingsService _settingsService;
		#endregion
	}
}
