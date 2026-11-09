using Effani.Device.DataCollection;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using DevExpress.Mvvm;
using Effani.Device.Services;
using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Mvvm.Xpf;
using Drivetrain.Robotics.Control;
using System.Windows.Input;
using Effani.Device.Control;
using Drivetrain.Robotics.Process;
using DevExpress.Xpf.Core;

namespace Effani.Device.UI
{
	public partial class UtilitiesViewModel : ViewModelBase, IDisposable
	{
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
			var plate = _config.DeviceSpecification.Plates.First();
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
			var plate = _config.DeviceSpecification.Plates.First();
			var well = plate.Wells.Last();
			CurrentWell = well;
			if (parameter is AxisSpec axisSpec)
				_processEngine.QueueCommand(new AlignWellToAxis(axisSpec.Axis, well, _device));
			else if (parameter is SensorSpec sensorSpec)
				_processEngine.QueueCommand(new AlignWellToLocation(sensorSpec.Location, well, _device));
		}
		[Command]
		public async Task GenerateConfigFiles()
		{
			try
			{
				var fileLocation = _config
					.GenerateConfigFiles(Environment.GetFolderPath(Environment.SpecialFolder.Desktop));
				string messageBoxText = $"Config generated at {fileLocation}";
				var warnings = _config.GetConnectionWarnings();
				if (warnings.Count > 0)
					messageBoxText += $"\n\nWARNING - check the board connections in Electronic Settings:\n- {string.Join("\n- ", warnings)}";
				string caption = "Generate Config";

				DXMessageBox.Show(messageBoxText, caption);
			}
			catch (Exception ex)
			{
				var a = ex;
			}
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
			var distance = Convert.ToDecimal(parameters[1]);
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
		public async Task TrayUvOff()
		{
			_processEngine.QueueCommand(new TrayUvOff(_device));
		}
		[Command]
		public async Task TrayUvOn()
		{
			_processEngine.QueueCommand(new TrayUvOn(_device));
		}
		[Command]
		public void PrimeFluid()
		{
			var job = new Job("Priming fluid pumps.");
			job.Commands.Add(new EmitFluid(Axis.DnaReagent, _config.Settings.InitialDnaFluidPrimingVolume, _device));
			job.Commands.Add(new EmitFluid(Axis.Koh, _config.Settings.InitialKohFluidPrimingVolume, _device));
			job.Commands.Add(new EmitFluid(Axis.Nadh, _config.Settings.InitialNadhFluidPrimingVolume, _device));
			job.Commands.Add(new EmitFluid(Axis.Basal, _config.Settings.InitialBasalFluidPrimingVolume, _device));
			_processEngine.QueueJob(job);
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
		public async Task AlignToNextWell(object parameter)
		{
			var plate = _config.DeviceSpecification.Plates.First();
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
		public async Task EmitFluid(AxisSpec axisSpec)
		{
			_processEngine.QueueCommand(new EmitFluid(axisSpec.Axis, EmitFluidVolume, _device));
		}
		[Command]
		public async Task ReprimeIfNeeded(AxisSpec axisSpec)
		{
			_processEngine.QueueCommand(new ReprimeIfNeeded(axisSpec.Axis, _device));
		}
		[Command]
		public async Task ApplyConfig()
		{
			_device.ApplyConfig();
		}
		[Command]
		public async Task Aspirate(AxisSpec axisSpec)
		{
			_processEngine.QueueCommand(new Aspirate(_config.Settings.KohAspirationVolume, _device));
		}
		[Command]
		public async Task UploadSettings()
		{
			await _settingsService.UploadSettings();
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
	}
}
