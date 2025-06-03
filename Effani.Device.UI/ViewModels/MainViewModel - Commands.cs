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
		public async Task AlignForSterilization()
		{
			_processEngine.QueueCommand(new TableToTraySterilizationLocation(_device));
		}
		[Command]
		public async Task AlignForTrayLoading()
		{
			_processEngine.QueueCommand(new TableToTrayLoadingLocation(_device));
		}
		[Command]
		public async Task AlignForWaste()
		{
			_processEngine.QueueCommand(new AlignTableForWaste(_device));
		}
		[Command]
		public async Task AlignToFirstWell(object parameter)
		{
			var plate = _device.Configuration.DeviceSpecification.Plates.First();
			var well = plate.Wells.First();
			CurrentWell = well;
			if (parameter is AxisSpec axisSpec)
				_processEngine.QueueCommand(new AlignWellToAxis(axisSpec.Axis, well, _device));
			else if (parameter is SensorSpec sensorSpec)
				_processEngine.QueueCommand(new AlignWellToLocation(sensorSpec.Location, well, _device));
		}
		[Command]
		public async Task AlignToLastWell(object parameter)
		{
			var plate = _device.Configuration.DeviceSpecification.Plates.First();
			var well = plate.Wells.Last();
			CurrentWell = well;
			if (parameter is AxisSpec axisSpec)
				_processEngine.QueueCommand(new AlignWellToAxis(axisSpec.Axis, well, _device));
			else if (parameter is SensorSpec sensorSpec)
				_processEngine.QueueCommand(new AlignWellToLocation(sensorSpec.Location, well, _device));
		}
		[Command]
		public async Task AlignToNextWell(object parameter)
		{
			var plate = _device.Configuration.DeviceSpecification.Plates.First();
			if (CurrentWell == null)
				CurrentWell = plate.Wells.First();
			var nextIndex = plate.Wells.IndexOf(CurrentWell) + 1;
			if (plate.Wells.Count - 1 < nextIndex)
				return;
			var well = plate.Wells[nextIndex];
			CurrentWell = well;
			if (parameter is AxisSpec axisSpec)
				_processEngine.QueueCommand(new AlignWellToAxis(axisSpec.Axis, well, _device));
			else if (parameter is SensorSpec sensorSpec)
				_processEngine.QueueCommand(new AlignWellToLocation(sensorSpec.Location, well, _device));
		}
		[Command]
		public async Task ApplyConfig()
		{
			_device.ApplyConfig();
		}
		[Command]
		public async Task AspirateKoh(AxisSpec axisSpec)
		{
			_processEngine.QueueCommand(new AspirateKoh(_config.KohAspirationVolume, _device));
		}
		[Command]
		public void NextPage()
		{
			AdvanceWizardPage?.Invoke(this, new EventArgs());
		}
		[Command]
		public async Task UploadAssays()
		{
			await _assayManagerService.UploadAssays();
		}
		[Command]
		public async Task ChamberUvOff()
		{
			_processEngine.QueueCommand(new ChamberUvOff(_device));
		}
		[Command]
		public async Task ChamberUvOn()
		{
			_processEngine.QueueCommand(new ChamberUvOn(_device));
		}
		[Command]
		public async Task ClearMessages()
		{
			ConsoleMessages.Clear();
		}
		[Command]
		public async Task DisableCommands()
		{
			_device.Configuration.BypassDeviceCommands = 0;
		}
		[Command]
		public async Task EmitFluid(AxisSpec axisSpec)
		{
			var plate = _device.Configuration.DeviceSpecification.Plates
				.FirstOrDefault(p => p.Location.X == 0 && p.Location.Y == 0);
			_processEngine.QueueCommand(new EmitFluid(axisSpec.Axis, EmitFluidVolume, _device));
		}
		[Command]
		public async Task EnableCommands()
		{
			_device.Configuration.BypassDeviceCommands = 2000;
		}
		[Command]
		public void FluidLoaded()
		{
			FluidLoadingComplete = true;
			var job = new Job("Priming fluid pumps.");
			job.Commands.Add(new EmitFluid(Axis.DnaReagent, _device.Configuration.InitialDnaFluidPrimingVolume, _device));
			job.Commands.Add(new EmitFluid(Axis.Koh, _device.Configuration.InitialKohFluidPrimingVolume, _device));
			job.Commands.Add(new EmitFluid(Axis.Nadh, _device.Configuration.InitialNadhFluidPrimingVolume, _device));
			job.Commands.Add(new EmitFluid(Axis.Basal, _device.Configuration.InitialBasalFluidPrimingVolume, _device));
			job.Commands.Add(new NotifyComplete((command) => { NotifyFluidPrimingComplete(); }, "Notify fluid priming complete.", _device));
			_processEngine.QueueJob(job);
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
		public async Task GetBoardDiagnostics(int boardNumber)
		{
			_processEngine.QueueCommand(new GetBoardDiagnostics(boardNumber, _device));
		}
		[Command]
		public async Task Home(AxisSpec axisSpec)
		{
			_processEngine.QueueCommand(new Home(_device, axisSpec));
		}
		//[Command]
		//public async Task AspirateFluid(AxisSpec axisSpec)
		//{
		//	_processEngine.QueueCommand(new AspirateFluid(_config.TestMediaAspirationVolume, _device));
		//}
		[Command]
		public async Task HomeAll()
		{
			_processEngine.QueueCommand(new HomeAll(_device));
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
				job.Commands.Add(new EmitFluid(Axis.Basal, _config.SecondaryBasalFluidPrimingVolume, _device));
				Assay.IsPrimedBasal = true;
				_processEngine.QueuePriorityJob(job);
			}
			_processEngine.QueuePriorityJob(new Job($"Dispensing {_device.Configuration.BasalVolume}µg basal media to {CurrentLoadingSample.Label}.", [
				new AlignWellToAxis(Axis.Basal, CurrentLoadingSample.Well, _device),
				new EmitFluid(Axis.Basal, _device.Configuration.BasalVolume, _device)
			]));
			_processEngine.QueuePriorityJob(new Job($"Aligning {CurrentLoadingSample.Label} for biopsy sample loading.", [
				new AlignWellToLocation(_device.Configuration.BiopsyLocation, CurrentLoadingSample.Well, _device),
				new NotifyComplete((command) => { NotifyPreparedToLoadSample(); }, "Notify ready to load sample.", _device)
			]));
		}
		[Command]
		public async Task InjectSampleTest()
		{
			var well = CurrentWell;
			_processEngine.QueuePriorityJob(new Job($"Adding Basal media to {CurrentLoadingSample.Label}.", [
				new AlignWellToAxis(Axis.Basal, CurrentLoadingSample.Well, _device),
				new EmitFluid(Axis.Basal, _device.Configuration.BasalVolume, _device)
			]));
			_processEngine.QueuePriorityJob(new Job($"Aligning {CurrentLoadingSample.Label} for biopsy sample loading.", [
				new AlignWellToLocation(_device.Configuration.BiopsyLocation, CurrentLoadingSample.Well, _device)
			]));
		}
		[Command]
		public async Task LaserOff(SensorSpec sensor)
		{
			switch (sensor)
			{
				case DnaSensorSpec:
					_device.LaserDnaOff();
					break;
				case NadhSensorSpec:
					_device.LaserNadhOff();
					break;
			}
		}
		[Command]
		public async Task LaserOn(SensorSpec sensor)
		{
			switch (sensor)
			{
				case DnaSensorSpec:
					_device.LaserDnaOn();
					break;
				case NadhSensorSpec:
					_device.LaserNadhOn();
					break;
			}
		}
		[Command]
		public async Task MoveRapid(List<object> parameters)
		{
			var axisSpec = (AxisSpec)parameters[0];
			var distance = (float)parameters[1];
			_processEngine.QueueCommand(new MoveRelative(distance, _device, axisSpec));
		}
		[Command]
		public async Task MoveToStart(AxisSpec axisSpec)
		{
			_processEngine.QueueCommand(new MoveToStartingLocation(axisSpec.Axis, _device));
		}
		[Command]
		public async Task MoveToStartingLocations()
		{
			_processEngine.QueueCommand(new MoveToStartingLocations(_device));
		}
		[Command]
		public void PrimeFluid()
		{
			var job = new Job("Priming fluid pumps.");
			job.Commands.Add(new EmitFluid(Axis.DnaReagent, _device.Configuration.InitialDnaFluidPrimingVolume, _device));
			job.Commands.Add(new EmitFluid(Axis.Koh, _device.Configuration.InitialKohFluidPrimingVolume, _device));
			job.Commands.Add(new EmitFluid(Axis.Nadh, _device.Configuration.InitialNadhFluidPrimingVolume, _device));
			job.Commands.Add(new EmitFluid(Axis.Basal, _device.Configuration.InitialBasalFluidPrimingVolume, _device));
			_processEngine.QueueJob(job);
		}
		[Command]
		public async Task Read(SensorSpec sensor)
		{
			Mouse.OverrideCursor = Cursors.Wait;
			switch (sensor)
			{
				case DnaSensorSpec:
					_processEngine.QueueCommand(new ReadDna(_device, null));
					break;
				case NadhSensorSpec:
					_processEngine.QueueCommand(new ReadNadh(_device, null));
					break;
			}
			Mouse.OverrideCursor = Cursors.Arrow;
		}
		[Command]
		public async Task Send()
		{
			_processEngine.QueueCommand(new SendCommand(_sendCommandText, _device));
		}
		[Command]
		public async Task SetWifi()
		{
			var ssid = ConfigurationManager.AppSettings["wifi-ssid"];
			var pass = ConfigurationManager.AppSettings["wifi-password"];
			_device.SetWifi(ssid, pass);
		}
		[Command]
		public void ShowSettings()
		{
			var settingsWindow = new UtilitiesView() { Title = "Settings" };
			settingsWindow.DataContext = this;
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
					$"Skipping samples after number {CurrentLoadingSample.TestIndex}, {CurrentLoadingSample.SampleId}, {CurrentLoadingSample.Label}", true);
				var currentIndex = Assay.AllTestWells.IndexOf(CurrentLoadingSample);
				foreach (var test in Assay.AllTestWells.Skip(currentIndex))
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
		[Command]
		public async Task TrayUvOff()
		{
			_processEngine.QueueCommand(new TrayUvOff(_device));
		}
		[Command]
		public async Task TrayUvOn()
		{
			_processEngine.QueueCommand(new TrayUvOn(_device));
		}
		#endregion
	}
}
