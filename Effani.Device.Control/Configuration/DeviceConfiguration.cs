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
		public DeviceConfiguration() 
		{
			BuildDeviceSpec();
		}
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
			foreach (var x in DeviceSpecification.Axes)
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
"
				);
			foreach (var x in DeviceSpecification.Axes)
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
			configTextBuilder.AppendLine(@"M308 S0 P""1.io6.in"" Y""linear-analog"" A""DNAsensor"" F0 B0 C10 ; DNA optical sensor");
			configTextBuilder.AppendLine(@"M308 S1 P""2.io6.in"" Y""linear-analog"" A""NADHsensor"" F0 B0 C10 ; NADH optical sensor");
			configTextBuilder.AppendLine(@"M308 S3 P""1.temp0"" Y""thermistor"" A""Tray Heater"" T100000 B4725 C7.06e-8 ; configure tray heat sensor");
			configTextBuilder.AppendLine("\r\n; Heaters");
			configTextBuilder.AppendLine(@"M950 H0 C""1.out0"" T0 ; create tray heater");
			configTextBuilder.AppendLine(@"M140 P0 H0 ; configure bed heater to use the heater^");
			configTextBuilder.AppendLine($"M143 H0 P0 T0 C0 S{TrayTemperature} A0 ; configure heater monitor");
			configTextBuilder.AppendLine(@"M307 H0 R2.43 D5.5 E1.35 K0.56 B1 ; configure model of heater");

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
				var direction = x.Driver.Direction == Direction.Forwards ? null : "-";
				var distance = x.Maximum - x.Minimum + 5;
				homeTextBuilder.Append(@$"
G91 ; relative positioning
G1 H1 {x.Designation}{direction}{distance} F{x.Driver.Endstop.FirstHomingSpeed} ; coarse home
G1 {x.Designation}5 F6000 ; move back 5mm
G1 H1 {x.Designation}{direction}{distance} F{x.Driver.Endstop.SecondHomingSpeed} ; fine home
G90 ; absolute positioning
				");
				File.WriteAllBytes(Path.Combine(folder, $"home{DeviceSpecification.Axes[i].Designation}.g"), Encoding.ASCII.GetBytes(homeTextBuilder.ToString()));
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
			DeviceSpecification.Axes.Add(new TableAxis(Axis.TableX, 0, 0, 312, EndstopLocation.LowEnd, direction: Direction.Backwards));
			var tabley = new TableAxis(Axis.TableY, 0, 1, 364, EndstopLocation.LowEnd, direction: Direction.Backwards);
			tabley.Driver.Endstop.FirstHomingSpeed = 1000;
			DeviceSpecification.Axes.Add(tabley);
			DeviceSpecification.Axes.AddRange(GetBioposyAxes());
			DeviceSpecification.Axes.AddRange(GetAspirateAxes());
			DeviceSpecification.Axes.Add(new FluidAxis(Axis.Dna1, 1, 3, Dna1Location));
			DeviceSpecification.Axes.Add(new FluidAxis(Axis.Dna2, 1, 4, Dna2Location));
			DeviceSpecification.Axes.Add(new FluidAxis(Axis.Koh2, 2, 0, Koh2Location));
			DeviceSpecification.Axes.Add(new FluidAxis(Axis.Koh1, 2, 1, Koh1Location));
			DeviceSpecification.Axes.Add(new FluidAxis(Axis.Nadh2, 2, 2, Nadh2Location));
			DeviceSpecification.Axes.Add(new FluidAxis(Axis.Nadh1, 2, 3, Nadh1Location));
			DeviceSpecification.Axes.Add(new FluidAxis(Axis.Basal2, 2, 4, Basal2Location));
			DeviceSpecification.Axes.Add(new FluidAxis(Axis.Basal1, 3, 0, Basal1Location));
			DeviceSpecification.Plates.Add(new Plate(new Coordinate(0, 0)));
			DeviceSpecification.Plates.Add(new Plate(new Coordinate(1, 0)));
			DeviceSpecification.Plates.Add(new Plate(new Coordinate(0, 1)));
			DeviceSpecification.Plates.Add(new Plate(new Coordinate(1, 1)));
			Plate currentPlate = DeviceSpecification.Plates[0];
			var wellNumberInPlate = 0;
			for (int index = 0; index < PlateCapacity * TestWellsPerPlate; index++)
			{
				for (int x = 0; x < HorizontalWellCount; x++)
				{
					for (int y = 0; y < VerticalWellCount; y++)
					{
						var location = new Coordinate(
							(x * WellOffset.X) + (currentPlate.Index.X * PlateOffset.X), 
							(y * WellOffset.Y) + (currentPlate.Index.Y * PlateOffset.Y));
						new Well(currentPlate, x, y, location);
					}
				}
				if (wellNumberInPlate == TestWellsPerPlate - 1)
				{
					if(currentPlate == DeviceSpecification.Plates.Last())
						break;

					wellNumberInPlate = 0;
					currentPlate = DeviceSpecification.Plates
						.ElementAt(DeviceSpecification.Plates.IndexOf(currentPlate) + 1);
				}
				else
				{
					wellNumberInPlate++;
				}
			}
		}
		private List<AxisSpec> GetBioposyAxes()
		{
			var ret = new List<AxisSpec>();
			ret.Add(new BiopsyAxis(Axis.BiopsyPri, 0, 2, BiopsyLocation));
			ret.Add(new ShortZAxis(Axis.BiopsySec, 0, 3));

			return ret;
		}
		private List<AxisSpec> GetAspirateAxes()
		{
			var ret = new List<AxisSpec>();
			ret.Add(new AspirateAxis(Axis.Aspirate1, 0, 4, Aspirate1Location));
			ret.Add(new ShortZAxis(Axis.Aspirate1Z, 1, 0));
			ret.Add(new AspirateAxis(Axis.Aspirate2, 1, 1, Aspirate2Location));
			ret.Add(new ShortZAxis(Axis.Aspirate2Z, 1, 2));

			return ret;
		}

		public float PlateCapacity { get; set; } = 4; // number of sample plates this device can accomodate
		public float TrayTemperature { get; set; } = 38.777778f; // in Celcius, target temperature to heat the tray to
		public float FluidSyringeVolumeMl { get; set; } = 60f;
		public float FluidSyringeLength { get; set; } = 90f; // mm length of a fluid syringe's contents
		public float FluidSyringeSpeed { get; set; } = 10f; // speed at which a fluid syringe should emitt its contents
		public float AspirateSyringeVolumeMl { get; set; } = 1f;
		public float AspirateSyringeTravelMm { get; set; } = 60f; // mm length of an aspirate syringe's contents
		public float AspirateSyringeSpeed { get; set; } = 10f; // speed at which an aspirate syringe should withdraw its contents
		public float AspirateTravel { get; set; } = 0; // total mm aspirate axes will descend/ascend
		public Coordinate Aspirate1Location { get; set; } = new(0, 0);
		public Coordinate Aspirate2Location { get; set; } = new(0, 0);
		public Coordinate Dna1Location { get; set; } = new(256.5f, 362.5f);
		public Coordinate Dna2Location { get; set; } = new(256.5f, 412.5f);
		public Coordinate Koh1Location { get; set; } = new(256.50f, 262.5f);
		public Coordinate Koh2Location { get; set; } = new(256.50f, 312.5f);
		public Coordinate Nadh1Location { get; set; } = new(417.5f, 262.5f);
		public Coordinate Nadh2Location { get; set; } = new(417.5f, 412.5f);
		public Coordinate Basal1Location { get; set; } = new(417.5f, 262.5f);
		public Coordinate Basal2Location { get; set; } = new(417.5f, 312.5f);
		public Coordinate BiopsyLocation { get; set; } = new(0, 0);
		public Coordinate PlateOffset { get; set; } = new(130.5f, 88.5f); // distance from well 0 of a tray to well 0 of the next tray
		public Coordinate DnaLaserLocation { get; set; } = new(0, 0);
		public Coordinate NadhLaserLocation { get; set; } = new(0, 0);
		public Coordinate WasteLocation { get; set; } = new(0, 0);
		public int VerticalWellCount { get; set; } = 8;
		public int HorizontalWellCount { get; set; } = 12;
		public int TestWellsPerPlate { get; set; } = 90; // total wells on a given plate to be used for testing
		public Coordinate WellOffset { get; set; } = new(9, 9); // distance between cells
		public float TraySterilizationTime { get; set; } = 0; // ammount of seconds tray will be UV sterilized
		public float BiospsySterilizationTime { get; set; } = 0; // ammount of seconds biopsy needle will be UV sterilized
		public float BiospsySpeed { get; set; } = 0; // speed at which the biopsy needle should descend/ascend
		public float BiopsyTravel { get; set; } = 0; // mm biopsy axis will descend/ascend
		public float BiopsyZTravel { get; set; } = 0; // mm biopsy Z axes will descend/ascend
		public float BiospsyZSpeed { get; set; } = 0; // speed at which the biopsy Z axis should descend/ascend

		public int FluidPrimingVolume { get; set; } = 500; // microliters dispensed during fluid pump priming
		public int BaselineProcessingDwell { get; set; } = 0; // ammount of milliseconds baseline processing will wait

		public int DnaVolume { get; set; } = 250; // microliters dispensed by DNA pump

		public int NadhVolume { get; set; } = 300; // microliters dispensed by NADH pump

		public int OpticalSensorDwell { get; set; } = 3000; // total milliseconds to activate laser and read sensor

		public int SensorReadDwell { get; set; } = 3000; // total milliseconds to wait for a sensor reading to be reported

		public int Co2Dwell { get; set; } = 500; // total milliseconds to activate CO2

		public int TestMediaAspirationVolume { get; set; } = 350; // microliters withdraw from baseline

		public float StirringDistance { get; set; } = 1; // total mm to move needle when stirring

		public int TestMediaTimerPeriod { get; set; } = 3600; // seconds the test media timers will wait

		public int KohTimerPeriod { get; set; } = 3600 * 4; // seconds the KOH timer will wait

		public int DnaTimerPeriod { get; set; } = 180; // seconds the DNA timer will wait

		public int KohAspirationVolume { get; set; } = 250; // microliters withdraw for KOH

		public int DnaAspirationVolume { get; set; } = 350; // microliters withdraw for DNA

		public DeviceSpecification? DeviceSpecification { get; private set; }
	}
}
