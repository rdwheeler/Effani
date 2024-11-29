using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Effani.Device.Control
{
	public class DeviceConfiguration
	{
		public DeviceConfiguration() { }
		public string GenerateConfigFiles(string drive)
		{
			try
			{
				var mainFolder = Path.Combine(drive, @"Board Configurations\Main Board");
				var sysFolder = Path.Combine(mainFolder, @"sys");
				WriteConfig(sysFolder);
				WriteHome(sysFolder);
				var zipFile = Path.Combine(mainFolder, "config.zip");
				File.Delete(zipFile);
				ZipFile.CreateFromDirectory(sysFolder, zipFile);
				return zipFile;
			}
			catch (Exception ex)
			{
				return ex.Message;
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
M552 S0
M587 S""blah - 2.4Ghz"" P""B4forever!""
M552 S1 ; configure WiFi adapter
M586 P0 S1 ; configure HTTP
M586 P1 S1 ; configure FTP
M586 P2 S1 ; configure Telnet

; Wait a moment for the CAN expansion boards to become available
G4 S4

; Smart Drivers
"
				);
			var spec = new DeviceConfiguration();
			var deviceSpec = spec.GetDeviceSpecification();
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
			configTextBuilder.Append($" ; set axis mapping\r\n");
			configTextBuilder.Append($"M350");
			foreach (var x in deviceSpec.Axes)
			{
				configTextBuilder.Append($" {x.Designation}16");
			}
			configTextBuilder.Append($" I1 ; configure microstepping with interpolation\r\n");
			configTextBuilder.Append($"M906");
			foreach (var x in deviceSpec.Axes)
			{
				configTextBuilder.Append($" {x.Designation}{x.Driver.Current}");
			}
			configTextBuilder.Append($" ; set axis driver currents\r\n");
			configTextBuilder.Append($"M92");
			foreach (var x in deviceSpec.Axes)
			{
				configTextBuilder.Append($" {x.Designation}{x.StepsPerMillimeter}");
			}
			configTextBuilder.Append($" ; configure steps per mm\r\n");
			configTextBuilder.Append($"M208");
			foreach (var x in deviceSpec.Axes)
			{
				configTextBuilder.Append($" {x.Designation}{x.Minimum}:{x.Maximum}");
			}
			configTextBuilder.Append($" ; set minimum and maximum axis limits\r\n");
			configTextBuilder.Append($"M566");
			foreach (var x in deviceSpec.Axes)
			{
				configTextBuilder.Append($" {x.Designation}900");
			}
			configTextBuilder.Append($" ; set maximum instantaneous speed changes (mm/min)\r\n");
			configTextBuilder.Append($"M203");
			foreach (var x in deviceSpec.Axes)
			{
				configTextBuilder.Append($" {x.Designation}{x.MaxSpeed}");
			}
			configTextBuilder.Append($" ; set maximum speeds (mm/min)\r\n");
			configTextBuilder.Append($"M201");
			foreach (var x in deviceSpec.Axes)
			{
				configTextBuilder.Append($" {x.Designation}500");
			}
			configTextBuilder.Append($" ; set accelerations (mm/s^2)\r\n");
			configTextBuilder.Append(@"

; Kinematics
M669 K0 ; configure Cartesian kinematics

; Endstops
"
				);
			foreach (var x in deviceSpec.Axes)
			{
				configTextBuilder.AppendLine($"M574 {x.Designation}{(x.Driver.Endstop.Location == EndstopLocation.LowEnd ? "1" : "2")} P\"{(x.Driver.Endstop.Signal == PinSignal.High ? null : "!")}{(x.DriverPort.StartsWith("0") ? null : (x.DriverPort.First()) + ".")}io{x.DriverPort.Last()}.in\" S1");
			}
			configTextBuilder.AppendLine("\r\n; Actuators");
			configTextBuilder.AppendLine(@"M950 P0 C""out1"" ; plate UV" );
			configTextBuilder.AppendLine(@"M950 P1 C""out3"" ; needle UV");
			configTextBuilder.AppendLine(@"M950 P2 C""out6"" ; DNA laser");
			configTextBuilder.AppendLine(@"M950 P3 C""out1"" ; NADH laser");
			configTextBuilder.AppendLine(@"M950 P4 C""out5"" ; CO2");
			configTextBuilder.AppendLine("\r\n; Sensors");
			configTextBuilder.AppendLine(@"M308 S0 P""1.io6.in"" Y""linear-analog"" A""DNAsensor"" F0 ; DNA optical sensor");
			configTextBuilder.AppendLine(@"M308 S1 P""2.io6.in"" Y""linear-analog"" A""NADHsensor"" F0 ; NADH optical sensor");

			File.WriteAllBytes(Path.Combine(folder, "config.g"), Encoding.ASCII.GetBytes(configTextBuilder.ToString()));
		}
		private void WriteHome(string folder)
		{
			var spec = new DeviceConfiguration();
			var deviceSpec = spec.GetDeviceSpecification();
			for (int i = 0; i < deviceSpec.Axes.Count; i++)
			{
				var homeTextBuilder = new StringBuilder();
				homeTextBuilder.Append(@"
; Home
"
				);
				var x = deviceSpec.Axes[i];
				var direction = x.Driver.Direction == Direction.Forwards ? null : "-";
				var distance = x.Maximum - x.Minimum + 5;
				homeTextBuilder.Append(@$"
G91 ; relative positioning
G1 H1 {x.Designation}{direction}{distance} F{x.Driver.Endstop.FirstHomingSpeed} ; coarse home
G1 {x.Designation}5 F6000 ; move back 5mm
G1 H1 {x.Designation}{direction}{distance} F{x.Driver.Endstop.SecondHomingSpeed} ; fine home
G90 ; absolute positioning
				");
				File.WriteAllBytes(Path.Combine(folder, $"home{deviceSpec.Axes[i].Designation}.g"), Encoding.ASCII.GetBytes(homeTextBuilder.ToString()));
			}
			var homeAllBuilder = new StringBuilder();
			foreach (var x in deviceSpec.Axes)
			{
				homeAllBuilder.AppendLine($"M98 P\"home{x.Designation}.g\" ; home {x.Label} axis");
			}
			File.WriteAllBytes(Path.Combine(folder, $"homeall.g"), Encoding.ASCII.GetBytes(homeAllBuilder.ToString()));
		}
		public DeviceSpecification GetDeviceSpecification()
		{
			var ret = new DeviceSpecification("Effani Device");
			ret.Axes.Add(new TableAxis(Axis.TableX, 0, 0, 312, EndstopLocation.LowEnd, direction: Direction.Backwards));
			var y = new TableAxis(Axis.TableY, 0, 1, 364, EndstopLocation.LowEnd, direction: Direction.Backwards);
			y.Driver.Endstop.FirstHomingSpeed = 1000;
			ret.Axes.Add(y);
			ret.Axes.AddRange(GetBioposyAxes());
			ret.Axes.AddRange(GetAspirateAxes());
			ret.Axes.Add(new FluidAxis(Axis.Dna1, 1, 3));
			ret.Axes.Add(new FluidAxis(Axis.Dna2, 1, 4));
			ret.Axes.Add(new FluidAxis(Axis.Koh2, 2, 0));
			ret.Axes.Add(new FluidAxis(Axis.Koh1, 2, 1));
			ret.Axes.Add(new FluidAxis(Axis.Nadh2, 2, 2));
			ret.Axes.Add(new FluidAxis(Axis.Nadh1, 2, 3));
			ret.Axes.Add(new FluidAxis(Axis.Basal2, 2, 4));
			ret.Axes.Add(new FluidAxis(Axis.Basal1, 3, 0));

			return ret;
		}

		public float FluidSyringeVolumeMl { get; set; } = 60f;
		public float FluidSyringeTravelMm { get; set; } = 90f;
		public float FluidSyringeSpeed { get; set; } = 10f;
		public float AspirateSyringeVolumeMl { get; set; } = 1f;
		public float AspirateSyringeTravelMm { get; set; } = 60f;
		public float AspirateSyringeSpeed { get; set; } = 10f;

		private List<AxisSpec> GetBioposyAxes()
		{
			var ret = new List<AxisSpec>();
			ret.Add(new BiopsyAxis(Axis.BiopsyPri, 0, 2));
			ret.Add(new ShortZAxis(Axis.BiopsySec, 0, 3));

			return ret;
		}
		private List<AxisSpec> GetAspirateAxes()
		{
			var ret = new List<AxisSpec>();
			ret.Add(new AspirateAxis(Axis.Aspirate1, 0, 4));
			ret.Add(new ShortZAxis(Axis.Aspirate1Z, 1, 0));
			ret.Add(new AspirateAxis(Axis.Aspirate2, 1, 1));
			ret.Add(new ShortZAxis(Axis.Aspirate2Z, 1, 2));

			return ret;
		}
	}
}
