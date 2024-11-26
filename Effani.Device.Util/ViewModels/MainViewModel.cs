using DevExpress.Mvvm;
using Effani.Device.Control;
using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Effani.Device.Util.ViewModels
{
	public class MainViewModel : ViewModelBase
	{
		public MainViewModel()
		{
			GenerateConfigFilesCommand = new AsyncCommand(GenerateConfigFiles);
		}
		async Task GenerateConfigFiles()
		{
			while (!GenerateConfigFilesCommand.IsCancellationRequested)
			{
				var folder = Path.Combine("I:\DriveTrain\Effani\Effani.Device.Util", "Board Configurations/Main Board/sys");
				WriteConfig(folder);
				WriteHome(folder);
			}
		}

		private void WriteConfig(string folder)
		{
			var configTextBuilder = new StringBuilder();
			configTextBuilder.Append(@"
; General
G90 ; absolute coordinates
M83 ; relative extruder moves
M550 P""Effani"" ; set hostname

; Network
M552 S1 ; configure WiFi adapter
M586 P0 S1 ; configure HTTP
M586 P1 S1 ; configure FTP
M586 P2 S1 ; configure Telnet

; Wait a moment for the CAN expansion boards to become available
G4 S2

; Smart Drivers
"
				);
			var spec = new Specification();
			var deviceSpec = spec.GetDeviceSpecification("Effani Device");
			foreach (var x in deviceSpec.Axes)
			{
				configTextBuilder.AppendLine($"M569 P{x.DriverPort} S{(x.Driver.Direction == Direction.Forwards ? 1 : 0)} D2 ; {x.Label}");
			}
			configTextBuilder.Append(@"

; Motor Idle Current Reduction
M906 I30 ; set motor current idle factor
M84 S30 ; set motor current idle timeout

; Axes
"
				);
			configTextBuilder.Append($"M584");
			foreach (var x in deviceSpec.Axes)
			{
				configTextBuilder.Append($" {x.Designation}{x.DriverPort}");
			}
			configTextBuilder.Append($" ; set axis mapping");
			configTextBuilder.Append($"M350");
			foreach (var x in deviceSpec.Axes)
			{
				configTextBuilder.Append($" {x.Designation}16");
			}
			configTextBuilder.Append($" I1 ; configure microstepping with interpolation");
			configTextBuilder.Append($"M906");
			foreach (var x in deviceSpec.Axes)
			{
				configTextBuilder.Append($" {x.Designation}800");
			}
			configTextBuilder.Append($" ; set axis driver currents");
			configTextBuilder.Append($"M92");
			foreach (var x in deviceSpec.Axes)
			{
				configTextBuilder.Append($" {x.Designation}{x.StepsPerMillimeter}");
			}
			configTextBuilder.Append($" ; configure steps per mm");
			configTextBuilder.Append($"M208");
			foreach (var x in deviceSpec.Axes)
			{
				configTextBuilder.Append($" {x.Designation}{x.Minimum}{x.Maximum}");
			}
			configTextBuilder.Append($" ; set minimum and maximum axis limits");
			configTextBuilder.Append($"M566");
			foreach (var x in deviceSpec.Axes)
			{
				configTextBuilder.Append($" {x.Designation}900");
			}
			configTextBuilder.Append($" ; set maximum instantaneous speed changes (mm/min)");
			configTextBuilder.Append($"M203");
			foreach (var x in deviceSpec.Axes)
			{
				configTextBuilder.Append($" {x.Designation}{x.MaxSpeed}");
			}
			configTextBuilder.Append($" ; set maximum speeds (mm/min)");
			configTextBuilder.Append($"M203");
			foreach (var x in deviceSpec.Axes)
			{
				configTextBuilder.Append($" {x.Designation}500");
			}
			configTextBuilder.Append($" ; set accelerations (mm/s^2)");
			configTextBuilder.Append(@"

; Kinematics
M669 K0 ; configure Cartesian kinematics

; Endstops
"
				);
			configTextBuilder.Append($"M203");
			foreach (var x in deviceSpec.Axes)
			{
				configTextBuilder.AppendLine($"M574 {x.Designation}{(x.Driver.Endstop.Location == EndstopLocation.LowEnd ? "1" : "2")} P{(x.Driver.Endstop.Signal == PinSignal.High ? null : "!")}{(x.DriverPort.StartsWith("0") ? null : x.DriverPort.First())}io{x.DriverPort.Last()}.in S1");
			}
			File.WriteAllBytes(Path.Combine(folder, "config.g"), Encoding.ASCII.GetBytes(configTextBuilder.ToString()));
		}
		private void WriteHome(string folder)
		{
			var spec = new Specification();
			var deviceSpec = spec.GetDeviceSpecification("Effani Device");
			for (int i = 0; i < deviceSpec.Axes.Count; i++)
			{
				var homeTextBuilder = new StringBuilder();
				homeTextBuilder.Append(@"
; Home
G28 ; home all axes
"
				);
				var x = deviceSpec.Axes[i];
				homeTextBuilder.Append(@$"
G91 ; relative positioning
var maxTravel = move.axes[{i}].max - move.axes[{i}].min + 5 ; calculate how far {x.Designation} can travel plus 5mm
G1 H1 {x.Designation}-{(x.Maximum - x.Minimum + 5)} F{x.Driver.Endstop.FirstHomingSpeed} ; coarse home
G1 {x.Designation}5 F6000 ; move back 5mm
G1 H1 X-{(x.Maximum - x.Minimum + 5)} F{x.Driver.Endstop.SecondHomingSpeed} ; fine home
G90 ; absolute positioning
				");
				File.WriteAllBytes(Path.Combine(folder, $"home{deviceSpec.Axes[i].Designation}.g"), Encoding.ASCII.GetBytes(homeTextBuilder.ToString()));
			}
		}

		public IAsyncCommand GenerateConfigFilesCommand { get; }

		private const string xDriveAxis = "X";
	}
}
