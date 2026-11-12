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
		/// <summary>Reports the outcome of sending the generated files to the controller board; runs on the process engine thread.</summary>
		private void SendConfigFilesComplete(string? error)
		{
			Application.Current.Dispatcher.BeginInvoke(() =>
			{
				var now = DateTime.Now.ToString("H:mm:ss");
				if (error is null)
				{
					ConsoleMessages.Insert(0, $"{now} Generated config files sent to the controller board; the board was rebooted.");
					return;
				}
				ConsoleMessages.Insert(0, $"{now} Sending the generated config files to the controller board failed: {error}");
				DXMessageBox.Show($"The config files were generated but could not be sent to the controller board (it was not rebooted).\n\n{error}", "Send Config Files");
			});
		}
		[Command]
		public async Task GenerateConfigFiles()
		{
			try
			{
				// The active settings object is replaced when a settings version is activated or reset, so point the configuration at the current one.
				_config.Settings = _settingsService.Settings;
				var fileLocation = _config
					.GenerateConfigFiles(SettingsService.DataFolder);
				string messageBoxText = $"Config generated at {fileLocation}";
				var warnings = _config.GetConnectionWarnings();
				if (warnings.Count > 0)
					messageBoxText += $"\n\nWARNING - check these settings in Electronic Settings:\n- {string.Join("\n- ", warnings)}";
				string caption = "Generate Config";

				if (!System.IO.File.Exists(fileLocation))
				{
					// generation failed: fileLocation holds the error message
					DXMessageBox.Show(messageBoxText, caption);
					return;
				}
				if (IsAssayRunning)
						messageBoxText += "\n\nThe generated files were NOT sent to the controller board because an assay is running.";
					else
					{
						_processEngine.QueueCommand(new SendConfigFiles(_config.GeneratedFiles, SendConfigFilesComplete, _device));
						messageBoxText += "\n\nSending the generated files to the controller board, which will then be rebooted. The result will appear in the console messages.";
					}
					var openFolder = new UICommand { Id = "open", Caption = "Open Folder", IsDefault = true };
				var close = new UICommand { Id = "close", Caption = "Close", IsCancel = true };
				var dialog = new DXDialogWindow(caption, new[] { openFolder, close })
				{
					Content = new System.Windows.Controls.TextBlock { Text = messageBoxText, TextWrapping = TextWrapping.Wrap, MaxWidth = 600, Margin = new Thickness(15) },
					SizeToContent = SizeToContent.WidthAndHeight,
					WindowStartupLocation = WindowStartupLocation.CenterOwner,
					Owner = Application.Current.Windows.OfType<Window>().FirstOrDefault(w => w.IsActive) ?? Application.Current.MainWindow
				};
				if (dialog.ShowDialogWindow() == openFolder)
					System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo("explorer.exe", $"/select,\"{fileLocation}\"") { UseShellExecute = true });
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
		public async Task EnableWifi()
		{
			_processEngine.QueueCommand(new SetWifiEnabled(true, _device));
		}
		[Command]
		public async Task DisableWifi()
		{
			_processEngine.QueueCommand(new SetWifiEnabled(false, _device));
		}
		[Command]
		public async Task SetWifi()
		{
			if (string.IsNullOrWhiteSpace(Settings.WifiSsid))
			{
				DXMessageBox.Show("Enter a WiFi SSID first.", "Set WiFi");
				return;
			}
			_processEngine.QueueCommand(new SetWifi(Settings.WifiSsid, Settings.WifiPassword, _device));
		}
		[Command]
		public async Task CheckForUpdates()
		{
			if (_checkingForUpdates)
				return;
			_checkingForUpdates = true;
			try
			{
				UpdateStatus = "Checking for updates...";
				var current = typeof(UtilitiesViewModel).Assembly.GetName().Version ?? new Version(0, 0);
				var result = await _updateChecker.CheckAsync(current);
				UpdateStatus = result.Message;
				if (!result.IsUpdateAvailable)
					return;

				if (IsAssayRunning)
				{
					UpdateStatus += " Install it once the assay has finished.";
					return;
				}
				var prompt = $"{result.Message}\n\nThe updater will close this program to install it. Run the updater now?";
				if (DXMessageBox.Show(prompt, "Software Update", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
					UpdateChecker.StartUpdater();
			}
			catch (Exception ex)
			{
				UpdateStatus = $"Could not start the updater: {ex.Message}";
			}
			finally
			{
				_checkingForUpdates = false;
			}
		}
		[Command]
		public async Task Send()
		{
			_processEngine.QueueCommand(new SendCommand(_sendCommandText, _device));
		}
	}
}
