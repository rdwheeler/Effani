using DevExpress.CodeParser;
using DevExpress.Mvvm;
using DevExpress.Mvvm.CodeGenerators;
using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Xpf.CodeView;
using DevExpress.Xpf.Core;
using DevExpress.XtraScheduler.Commands;
using Drivetrain.Robotics.Control;
using Drivetrain.Robotics.Process;
using Effani.Device.DataCollection;
using Effani.Device.Control;
using Serilog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
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
		#region Public methods
		[Command]
		public void AcceptSample()
		{
			var job = new Job($"{CurrentLoadingSample.Well.Label} sample loaded.");
			job.Commands.Add(new StartTimer(CurrentLoadingSample.BasalIncubationTimer, $"{CurrentLoadingSample.Well.Label} basal incubation", _device));
			_processEngine.QueuePriorityJob(job);

			_processEngine.HoldQueueForHighPriorityJob = false;

			SamplesInBasal.Add(new TestWellViewModel(CurrentLoadingSample));
			UnloadedTestsCount--;
			PrepareNextSampleForLoading();
		}
		[Command]
		public void NextPage()
		{
			AdvanceWizardPage?.Invoke(this, new EventArgs());
		}
		[Command]
		public void FluidLoaded()
		{
			FluidLoadingComplete = true;
			var job = new Job("Priming fluid pumps.");
			job.Commands.Add(new EmitFluid(Axis.DnaReagent, _config.Settings.InitialDnaFluidPrimingVolume, _device));
			job.Commands.Add(new EmitFluid(Axis.Koh, _config.Settings.InitialKohFluidPrimingVolume, _device));
			job.Commands.Add(new EmitFluid(Axis.Nadh, _config.Settings.InitialNadhFluidPrimingVolume, _device));
			job.Commands.Add(new EmitFluid(Axis.Basal, _config.Settings.InitialBasalFluidPrimingVolume, _device));
			job.Commands.Add(new NotifyComplete((command) => { NotifyFluidPrimingComplete(); }, "Notify fluid priming complete.", _device));
			_processEngine.QueueJob(job);
		}
		[Command]
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
				new AlignWellToAxis(Axis.Basal, CurrentLoadingSample.Well, _device),
				new EmitFluid(Axis.Basal, _config.Settings.BasalVolume, _device)
			]));
			_processEngine.QueuePriorityJob(new Job($"Aligning {CurrentLoadingSample.Label} for biopsy sample loading.", [
				new AlignWellToLocation(_config.Settings.BiopsyLocation, CurrentLoadingSample.Well, _device),
				new NotifyComplete((command) => { NotifyPreparedToLoadSample(); }, "Notify ready to load sample.", _device)
			]));
		}
		[Command]
		public void ShowSettings()
		{
			var settingsWindow = new UtilitiesView(_settingsService, _processEngine, _device, ConsoleMessages) { Title = "Settings" };
			settingsWindow.ShowDialog();
		}
		[Command]
		public void ShowUpload()
		{
			var window = new UploadView(_assayManagerService, _settingsService) { Title = "Effani Online" };
			window.ShowDialog();
		}
		[Command]
		public void SkipRemaining()
		{
			_processEngine.HoldQueueForHighPriorityJob = false;

			if (CurrentLoadingSample is not null)
			{
				AddMessage(
					$"Skipping samples after number {CurrentLoadingSample.TestIndex}, {CurrentLoadingSample.AnimalId}, {CurrentLoadingSample.Label}", true);
				var currentIndex = Assay.AllTestWells.IndexOf(CurrentLoadingSample);
				foreach (var test in Assay.AllTestWells.Skip(currentIndex))
					test.Canceled = true;
				UnloadedTestsCount = 0;
				NumberOfSamples = currentIndex;
			}
			CurrentLoadingSample = null; // triggers the loading process is complete
			PreparedToLoadSample = false;
			UnloadedTestsCount = 0;
			if (SamplesComplete.Count() == NumberOfSamples)
			{
				AddMessage("Assay complete", true);
				AssayComplete = true;
			}
			AdvanceWizardPage?.Invoke(this, new EventArgs());
		}
		[Command]
		public void SkipSample()
		{
			_processEngine.HoldQueueForHighPriorityJob = false;

			AddMessage($"Skipping sample in {CurrentLoadingSample.Well.Label}", true);
			CurrentLoadingSample.Canceled = true;
			SamplesComplete.Add(new TestWellViewModel(CurrentLoadingSample));
			PrepareNextSampleForLoading();
		}
		[Command]
		public void TraysLoaded()
		{
			new System.Threading.Thread(() => Assay.EstablishStandards()).Start();
			AdvanceWizardPage?.Invoke(this, new EventArgs());
		}
		#endregion
	}
}
