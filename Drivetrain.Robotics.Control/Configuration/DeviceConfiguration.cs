using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Configuration;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Drivetrain.Robotics.Control
{
	public class DeviceConfiguration : ConfigurationSection
	{
		public DeviceConfiguration(Settings settings)
		{
			Settings = settings;
			BuildDeviceSpec();
		}
		public string GenerateConfigFiles(string drive)
		{
			try
			{
				var mainFolder = Path.Combine(drive, @"");
				var sysFolder = Path.Combine(mainFolder, @"sys");
				if(!Directory.Exists(sysFolder))
					Directory.CreateDirectory(sysFolder);
				WriteConfig(sysFolder);
				WriteHome(sysFolder);
				var zipFilePath = Path.Combine(mainFolder, "config.zip");
				File.Delete(zipFilePath);
				ZipFile.CreateFromDirectory(sysFolder, zipFilePath);
				return zipFilePath;
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
M550 P""ABAL"" ; set hostname

; Network
M552 S2 ; configure WiFi adapter
M586 P0 S1 ; configure HTTP
M586 P1 S1 ; configure FTP
M586 P2 S1 ; configure Telnet

; Wait a moment for the CAN expansion boards to become available
G4 S4

; Smart Drivers
"
				);
			foreach (var x in DeviceSpecification.Axes)
			{
				configTextBuilder.AppendLine($"M569 P{x.DriverPort} S{(x.Driver.Direction == Direction.Forwards ? 1 : 0)} D2 ; {x.Label} {x.Designation}");
			}
			configTextBuilder.Append(@"

; Motor Idle Current Reduction
M906 I30 ; set motor current idle factor
M84 S30 ; set motor current idle timeout

; Set movement queue
M595 P0

; Axes
"
				);
			configTextBuilder.Append($"M584");
			foreach (var x in DeviceSpecification.Axes)
			{
				configTextBuilder.Append($" {x.Designation}{x.DriverPort}");
			}
			configTextBuilder.Append($" ; set axis mapping\r\n");
			configTextBuilder.Append($"M350");
			foreach (var x in DeviceSpecification.Axes)
			{
				configTextBuilder.Append($" {x.Designation}16");
			}
			configTextBuilder.Append($" I1 ; configure microstepping with interpolation\r\n");
			configTextBuilder.Append($"M906");
			foreach (var x in DeviceSpecification.Axes)
			{
				configTextBuilder.Append($" {x.Designation}{x.Driver.Current}");
			}
			configTextBuilder.Append($" ; set axis driver currents\r\n");
			configTextBuilder.Append($"M92");
			foreach (var x in DeviceSpecification.Axes)
			{
				configTextBuilder.Append($" {x.Designation}{x.StepsPerMillimeter}");
			}
			configTextBuilder.Append($" ; configure steps per mm\r\n");
			configTextBuilder.Append($"M208");
			foreach (var x in DeviceSpecification.Axes)
			{
				configTextBuilder.Append($" {x.Designation}{x.Minimum}:{x.Maximum}");
			}
			configTextBuilder.Append($" ; set minimum and maximum axis limits\r\n");
			configTextBuilder.Append($"M566");
			foreach (var x in DeviceSpecification.Axes)
			{
				configTextBuilder.Append($" {x.Designation}900");
			}
			configTextBuilder.Append($" ; set maximum instantaneous speed changes (mm/min)\r\n");
			configTextBuilder.Append($"M203");
			foreach (var x in DeviceSpecification.Axes)
			{
				configTextBuilder.Append($" {x.Designation}{x.MaxSpeed}");
			}
			configTextBuilder.Append($" ; set maximum speeds (mm/min)\r\n");
			configTextBuilder.Append($"M201");
			foreach (var x in DeviceSpecification.Axes)
			{
				configTextBuilder.Append($" {x.Designation}500");
			}
			configTextBuilder.Append($" ; set accelerations (mm/s^2)\r\n");
			configTextBuilder.Append(@"

; Kinematics
M669 K0 ; configure Cartesian kinematics

; Endstops
");
            foreach (var x in DeviceSpecification.Axes)
            {
                configTextBuilder.AppendLine($"M574 {x.Designation}{(x.Driver.Endstop.Location == EndstopLocation.LowEnd ? "1" : "2")} P\"{(x.Driver.Endstop.Signal == PinSignal.High ? null : "!")}{x.Driver.Endstop.SensorPort}\" S1");
            }
            var t = configTextBuilder.ToString();
            configTextBuilder.AppendLine("\r\n; Actuators");
			configTextBuilder.AppendLine(@"M950 P0 C""2.out5"" ; tray UV");
			configTextBuilder.AppendLine(@"M950 P1 C""1.out6"" ; chamber UV");
			configTextBuilder.AppendLine(@"M950 P2 C""out6"" ; NADH laser");
			configTextBuilder.AppendLine(@"M950 P3 C""2.out6"" ; DNA laser");
			//var aspirationPressureToggleBoardNumber = AspirationPressureToggleBoardNumber == 0 ? null : $"{AspirationPressureToggleBoardNumber}.";
			//configTextBuilder.AppendLine(@$"M950 P4 C""{aspirationPressureToggleBoardNumber}out{AspirationPressureTogglePortNumber}"" ; aspiration emission switch");
			var aspirationSuctionToggleBoardNumber = Settings.AspirationSuctionToggleBoardNumber == 0 ? null : $"{Settings.AspirationSuctionToggleBoardNumber}.";
			configTextBuilder.AppendLine(@$"M950 P5 C""{aspirationSuctionToggleBoardNumber}out{Settings.AspirationSuctionTogglePortNumber}"" ; aspiration emission switch");
			configTextBuilder.AppendLine("\r\n; Sensors");
			foreach (var sensor in DeviceSpecification.Sensors)
			{
				configTextBuilder.AppendLine($@"M308 S{sensor.SensorNumber} P""{sensor.PortDesignation}"" Y""linear-analog"" A""{sensor.Label}"" F0 B{sensor.ReadLow} C{sensor.ReadHigh}");
			}
			//configTextBuilder.AppendLine(@"M308 S3 P""2.temp0"" Y""thermistor"" A""Tray Heater"" T10000 B3950 ; configure tray heat sensor");
			//configTextBuilder.AppendLine("\r\n; Heaters");
			//configTextBuilder.AppendLine(@"M950 H0 C""1.out0"" T3 ; create tray heater");
			//configTextBuilder.AppendLine($"M143 H0 P0 T0 C0 S100 A0 ; configure heater monitor");
			//configTextBuilder.AppendLine(@"M307 H0 R2.43 D5.5 E1.35 K0.56 B1 ; configure model of heater");
			//configTextBuilder.AppendLine(@"M950 H1 C""1.out1"" T3 ; create tray heater");
			//configTextBuilder.AppendLine($"M143 H1 P0 T1 C0 S100 A0 ; configure heater monitor");
			//configTextBuilder.AppendLine(@"M307 H1 R2.43 D5.5 E1.35 K0.56 B1 ; configure model of heater");
			//configTextBuilder.AppendLine(@"M950 H2 C""2.out0"" T3 ; create tray heater");
			//configTextBuilder.AppendLine($"M143 H2 P0 T2 C0 S100 A0 ; configure heater monitor");
			//configTextBuilder.AppendLine(@"M307 H2 R2.43 D5.5 E1.35 K0.56 B1 ; configure model of heater");
			//configTextBuilder.AppendLine(@"M950 H3 C""2.out1"" T3 ; create tray heater");
			//configTextBuilder.AppendLine($"M143 H3 P0 T3 C0 S100 A0 ; configure heater monitor");
			//configTextBuilder.AppendLine(@"M307 H3 R2.43 D5.5 E1.35 K0.56 B1 ; configure model of heater");
			//configTextBuilder.AppendLine("\r\n; Heated beds");
			//configTextBuilder.AppendLine(@"M140 P0 H0 ; configure bed heater to use the heater^");
			//configTextBuilder.AppendLine(@"M140 P1 H1 ; configure bed heater to use the heater^");
			//configTextBuilder.AppendLine(@"M140 P2 H2 ; configure bed heater to use the heater^");
			//configTextBuilder.AppendLine(@"M140 P3 H3 ; configure bed heater to use the heater^");

			File.WriteAllBytes(Path.Combine(folder, "config.g"), Encoding.ASCII.GetBytes(configTextBuilder.ToString()));
		}
		private void WriteHome(string folder)
		{
			for (int i = 0; i < DeviceSpecification.Axes.Count; i++)
			{
				var homeTextBuilder = new StringBuilder();
				homeTextBuilder.Append(@"
; Home
"
				);
				var x = DeviceSpecification.Axes[i];
				var direction = x.Driver.Endstop.Location == EndstopLocation.HighEnd ? null : "-";
				var distance = x.Maximum - x.Minimum + 5;
				homeTextBuilder.Append(@$"
G91 ; relative positioning
G1 H1 {x.Designation}{direction}{distance} F{x.Driver.Endstop.FirstHomingSpeed} ; coarse home
G1 {x.Designation}5 F6000 ; move back 5mm
G1 H1 {x.Designation}{direction}{distance} F{x.Driver.Endstop.SecondHomingSpeed} ; fine home
G90 ; absolute positioning
				");
				File.WriteAllBytes(Path.Combine(folder, $"home{x.Designation}.g"), Encoding.ASCII.GetBytes(homeTextBuilder.ToString()));
			}
			var homeAllBuilder = new StringBuilder();
			foreach (var x in DeviceSpecification.Axes)
			{
				homeAllBuilder.AppendLine($"M98 P\"home{x.Designation}.g\" ; home {x.Label} axis");
			}
			File.WriteAllBytes(Path.Combine(folder, $"homeall.g"), Encoding.ASCII.GetBytes(homeAllBuilder.ToString()));
		}

		private void BuildDeviceSpec()
		{
			DeviceSpecification = new DeviceSpecification();
			DeviceSpecification.DeviceName = DeviceSpecification.Label;
			DeviceSpecification.Axes.Add(new TableAxis(Axis.TableX, 0, 0, 500, EndstopLocation.LowEnd, Direction.Backwards, Settings.TableAxisSpeed));
			DeviceSpecification.Axes.Add((AxisSpec)new TableAxis(Axis.TableY, 0, 1, 500, EndstopLocation.LowEnd, Direction.Backwards, Settings.TableAxisSpeed));
			DeviceSpecification.Axes.AddRange(GetAspirateAxes());
			DeviceSpecification.Axes.Add(new FluidAxis(Axis.DnaReagent, 1, 4, Settings.DnaLocation, Settings.DnaSyringeLoadingPosition));
			DeviceSpecification.Axes.Add(new FluidAxis(Axis.Koh, 2, 1, Settings.KohLocation, Settings.KohSyringeLoadingPosition));
			DeviceSpecification.Axes.Add(new FluidAxis(Axis.Nadh, 2, 3, Settings.NadhLocation, Settings.NadhSyringeLoadingPosition));
			var basal1 = new FluidAxis(Axis.Basal, 0, 5, Settings.BasalLocation, Settings.BasalSyringeLoadingPosition);
            basal1.Driver.Endstop.SensorPort = ($"1.io0.in");
            DeviceSpecification.Axes.Add(basal1);

			DeviceSpecification.Sensors.Add(new DnaSensorSpec("DNA Sensor", Settings.DnaSensorNumber, Settings.DnaSensorBoardNumber, Settings.DnaSensorPortNumber, Settings.DnaSensorReadHigh, Settings.DnaSensorReadLow, Settings.DnaSensorLocation));
			DeviceSpecification.Sensors.Add(new NadhSensorSpec("NADH Sensor", Settings.NadhSensorNumber, Settings.NadhSensorBoardNumber, Settings.NadhSensorPortNumber, Settings.NadhSensorReadHigh, Settings.NadhSensorReadLow, Settings.NadhSensorLocation));

			DeviceSpecification.Plates.Add(new Plate(new Coordinate(0, 0)));
			DeviceSpecification.Plates.Add(new Plate(new Coordinate(1, 0)));
			Plate currentPlate = DeviceSpecification.Plates[0];
			var wellsPerPlate = (Settings.HorizontalWellCount * Settings.VerticalWellCount);
			for (int index = 0; index < 2 * wellsPerPlate; index++)
			{
				for (int x = 0; x < Settings.HorizontalWellCount; x++)
				{
					for (int y = 0; y < Settings.VerticalWellCount; y++)
					{
                        var baseX = Settings.FirstWellLocation.X + (Settings.PlateOffset.X * currentPlate.Location.X);
                        var baseY = Settings.FirstWellLocation.Y + (Settings.PlateOffset.Y * currentPlate.Location.Y);
						var location = new Coordinate(
							((x * Settings.WellOffset.X) + baseX), 
							((y * Settings.WellOffset.Y) + baseY));
						new Well(currentPlate, x, y, location);
					}
				}
				if (currentPlate.Wells.Count() == wellsPerPlate)
				{
					if(currentPlate == DeviceSpecification.Plates.Last())
						break;

					currentPlate = DeviceSpecification.Plates
						.ElementAt(DeviceSpecification.Plates.IndexOf(currentPlate) + 1);
				}
			}
		}
		private List<AxisSpec> GetAspirateAxes()
		{
			var ret = new List<AxisSpec>();
			//ret.Add(new AspirateZAxis(Axis.AspirateFluid, 0, 4, Settings.AspirateFluidLocation, Settings.AspirateFluidActuatorStartPosition));
			var aspirate1Z = new AspirateZAxis(Axis.AspirateKoh, 1, 1, Settings.AspirateKohLocation, Settings.AspirateKohActuatorStartPosition);
            aspirate1Z.Driver.Endstop.SensorPort = ($"1.io1.in");
            ret.Add(aspirate1Z);

            return ret;
		}
		private Coordinate GetCoordinateConfigValue(string key)
		{
			var value = ConfigurationManager.AppSettings[key];
			if (value is null)
				throw new ValidationException($"Configuration key {key} not found");
			var x = decimal.Parse(value.Split(',')[0]);
			var y = decimal.Parse(value.Split(',')[1]);
			return new Coordinate(x, y);
		}

		public Settings Settings { get; set; }
		public DeviceSpecification? DeviceSpecification { get; private set; }
	}
}
