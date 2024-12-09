using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Configuration;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Effani.Device.Control
{
	public class DeviceConfiguration : ConfigurationSection
	{
		public DeviceConfiguration() 
		{
			SetConfigValues();
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
				configTextBuilder.AppendLine($"M569 P{x.DriverPort} S{(x.Driver.Direction == Direction.Forwards ? 1 : 0)} D2 ; {x.Label} {x.Designation}");
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
			//foreach (var x in DeviceSpecification.Axes)
			//{
			//	configTextBuilder.AppendLine($"M574 {x.Designation}{(x.Driver.Endstop.Location == EndstopLocation.LowEnd ? "1" : "2")} P\"{(x.Driver.Endstop.Signal == PinSignal.High ? null : "!")}{(x.DriverPort.StartsWith("0") ? null : (x.DriverPort.First()) + ".")}io{x.DriverPort.Last()}.in\" S1");
            //}
            foreach (var x in DeviceSpecification.Axes)
            {
                configTextBuilder.AppendLine($"M574 {x.Designation}{(x.Driver.Endstop.Location == EndstopLocation.LowEnd ? "1" : "2")} P\"{(x.Driver.Endstop.Signal == PinSignal.High ? null : "!")}{x.Driver.Endstop.SensorPort}\" S1");
            }
            var t = configTextBuilder.ToString();
            configTextBuilder.AppendLine("\r\n; Actuators");
			configTextBuilder.AppendLine(@"M950 P0 C""out1"" ; plate UV" );
			configTextBuilder.AppendLine(@"M950 P1 C""out3"" ; needle UV");
			configTextBuilder.AppendLine(@"M950 P2 C""out6"" ; NADH laser");
			configTextBuilder.AppendLine(@"M950 P3 C""2.out6"" ; DNA laser");
			configTextBuilder.AppendLine(@"M950 P4 C""out5"" ; CO2");
			configTextBuilder.AppendLine("\r\n; Sensors");
			configTextBuilder.AppendLine(@"M308 S0 P""1.io6.in"" Y""linear-analog"" A""DNAsensor"" F0 B0 C1 ; DNA optical sensor");
			configTextBuilder.AppendLine(@"M308 S1 P""2.io6.in"" Y""linear-analog"" A""NADHsensor"" F0 B0 C1 ; NADH optical sensor");
			configTextBuilder.AppendLine(@"M308 S3 P""2.temp0"" Y""thermistor"" A""Tray Heater"" T10000 B3950 ; configure tray heat sensor");
			configTextBuilder.AppendLine("\r\n; Heaters");
			configTextBuilder.AppendLine(@"M950 H0 C""1.out0"" T3 ; create tray heater");
			configTextBuilder.AppendLine($"M143 H0 P0 T0 C0 S100 A0 ; configure heater monitor");
			configTextBuilder.AppendLine(@"M307 H0 R2.43 D5.5 E1.35 K0.56 B1 ; configure model of heater");
			configTextBuilder.AppendLine(@"M950 H1 C""1.out1"" T3 ; create tray heater");
			configTextBuilder.AppendLine($"M143 H1 P0 T1 C0 S100 A0 ; configure heater monitor");
			configTextBuilder.AppendLine(@"M307 H1 R2.43 D5.5 E1.35 K0.56 B1 ; configure model of heater");
			configTextBuilder.AppendLine(@"M950 H2 C""2.out0"" T3 ; create tray heater");
			configTextBuilder.AppendLine($"M143 H2 P0 T2 C0 S100 A0 ; configure heater monitor");
			configTextBuilder.AppendLine(@"M307 H2 R2.43 D5.5 E1.35 K0.56 B1 ; configure model of heater");
			configTextBuilder.AppendLine(@"M950 H3 C""2.out1"" T3 ; create tray heater");
			configTextBuilder.AppendLine($"M143 H3 P0 T3 C0 S100 A0 ; configure heater monitor");
			configTextBuilder.AppendLine(@"M307 H3 R2.43 D5.5 E1.35 K0.56 B1 ; configure model of heater");
			configTextBuilder.AppendLine("\r\n; Heated beds");
			configTextBuilder.AppendLine(@"M140 P0 H0 ; configure bed heater to use the heater^");
			configTextBuilder.AppendLine(@"M140 P1 H1 ; configure bed heater to use the heater^");
			configTextBuilder.AppendLine(@"M140 P2 H2 ; configure bed heater to use the heater^");
			configTextBuilder.AppendLine(@"M140 P3 H3 ; configure bed heater to use the heater^");

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
			DeviceSpecification.Axes.Add(new TableAxis(Axis.TableX, 0, 0, 535, EndstopLocation.LowEnd, direction: Direction.Forwards));
			var tabley = new TableAxis(Axis.TableY, 0, 1, 516, EndstopLocation.LowEnd, direction: Direction.Backwards);
			tabley.Driver.Endstop.FirstHomingSpeed = 400;
			DeviceSpecification.Axes.Add(tabley);
			DeviceSpecification.Axes.AddRange(GetBioposyAxes());
			DeviceSpecification.Axes.AddRange(GetAspirateAxes());
			//DeviceSpecification.Axes.Add(new FluidAxis(Axis.DnaReagent2, 1, 3, Dna2Location));
			DeviceSpecification.Axes.Add(new FluidAxis(Axis.DnaReagent1, 1, 4, Dna1Location));
			//DeviceSpecification.Axes.Add(new FluidAxis(Axis.Koh2, 2, 0, Koh2Location));
			DeviceSpecification.Axes.Add(new FluidAxis(Axis.Koh1, 2, 1, Koh1Location));
			//DeviceSpecification.Axes.Add(new FluidAxis(Axis.Nadh2, 2, 2, Nadh2Location));
			DeviceSpecification.Axes.Add(new FluidAxis(Axis.Nadh1, 2, 3, Nadh1Location));
			//DeviceSpecification.Axes.Add(new FluidAxis(Axis.Basal2, 2, 4, Basal2Location));
            var basal1 = new FluidAxis(Axis.Basal1, 0, 5, Basal1Location);
            basal1.Driver.Endstop.SensorPort = ($"1.io0.in");
            DeviceSpecification.Axes.Add(basal1);

			DeviceSpecification.Plates.Add(new Plate(new Coordinate(0, 0)));
			DeviceSpecification.Plates.Add(new Plate(new Coordinate(0, 1)));
			//DeviceSpecification.Plates.Add(new Plate(new Coordinate(1, 0)));
			//DeviceSpecification.Plates.Add(new Plate(new Coordinate(1, 1)));
			Plate currentPlate = DeviceSpecification.Plates[0];
			for (int index = 0; index < PlateCapacity * TestWellsPerPlate; index++)
			{
				for (int y = 1; y < VerticalWellCount; y++) // skip first well row
                {
					for (int x = 0; x < HorizontalWellCount; x++) // skip first well row
					{
                        var baseX = FirstWellLocation.X + (PlateOffset.X * currentPlate.Index.X);
                        var baseY = FirstWellLocation.Y + (PlateOffset.Y * currentPlate.Index.Y);
						var location = new Coordinate(
							((x * WellOffset.X) + baseX), 
							((y * WellOffset.Y) + baseY));
						new Well(currentPlate, x, y, location);
					}
				}
				if (currentPlate.Wells.Count() == TestWellsPerPlate)
				{
					if(currentPlate == DeviceSpecification.Plates.Last())
						break;

					currentPlate = DeviceSpecification.Plates
						.ElementAt(DeviceSpecification.Plates.IndexOf(currentPlate) + 1);
				}
			}
		}
		private List<AxisSpec> GetBioposyAxes()
		{
			var ret = new List<AxisSpec>();
			ret.Add(new BiopsyAxis(Axis.Biopsy, 0, 2, BiopsyLocation));
			ret.Add(new BiopsyZAxis(Axis.BiopsyEject, 0, 3));

			return ret;
		}
		private List<AxisSpec> GetAspirateAxes()
		{
			var ret = new List<AxisSpec>();
			//ret.Add(new AspirateAxis(Axis.AspirateFluid, 1, 1, AspirateFluidLocation)); // asperate 2
			//ret.Add(new AspirateZAxis(Axis.AspirateFluidZ, 1, 2));
			ret.Add(new AspirateAxis(Axis.AspirateKoh, 0, 4, AspirateKohLocation)); // asperate 1
            var aspirate1Z = new AspirateZAxis(Axis.AspirateKohZ, 1, 0);
            aspirate1Z.Driver.Endstop.SensorPort = ($"1.io5.in");
            ret.Add(aspirate1Z);

            return ret;
		}
		private T GetConfigValue<T>(string key)
		{
			var value = ConfigurationManager.AppSettings[key];
			if (value is null)
				throw new ValidationException($"Configuration key {key} not found");

			return (T)Convert.ChangeType(value, typeof(T));
		}
		private Coordinate GetCoordinateConfigValue(string key)
		{
			var value = ConfigurationManager.AppSettings[key];
			if (value is null)
				throw new ValidationException($"Configuration key {key} not found");
			var x = float.Parse(value.Split(',')[0]);
			var y = float.Parse(value.Split(',')[1]);
			return new Coordinate(x, y);
		}
		private void SetConfigValues()
		{
			FluidSyringeHomePosition = GetConfigValue<float>(nameof(FluidSyringeHomePosition));
			AspirateFluidSyringeHomePosition = GetConfigValue<float>(nameof(AspirateFluidSyringeHomePosition));
			AspirateKohSyringeHomePosition = GetConfigValue<float>(nameof(AspirateKohSyringeHomePosition));
			PlateCapacity = GetConfigValue<float>(nameof(PlateCapacity));
			TrayTemperature = GetConfigValue<float>(nameof(TrayTemperature));
			FluidSyringeVolumeMl = GetConfigValue<float>(nameof(FluidSyringeVolumeMl));
			FluidSyringeLength = GetConfigValue<float>(nameof(FluidSyringeLength));
			FluidSyringeSpeed = GetConfigValue<float>(nameof(FluidSyringeSpeed));
			AspirateSyringeVolumeMl = GetConfigValue<float>(nameof(AspirateSyringeVolumeMl));
			AspirateSyringeTravelMm = GetConfigValue<float>(nameof(AspirateSyringeTravelMm));
			AspirateSyringeSpeed = GetConfigValue<float>(nameof(AspirateSyringeSpeed));
			AspirateTopZTravel = GetConfigValue<float>(nameof(AspirateTopZTravel));
			CameraLocation = GetCoordinateConfigValue(nameof(CameraLocation));
			AspirateFluidLocation = GetCoordinateConfigValue(nameof(AspirateFluidLocation));
			AspirateKohLocation = GetCoordinateConfigValue(nameof(AspirateKohLocation));
			Dna1Location = GetCoordinateConfigValue(nameof(Dna1Location));
			Dna2Location = GetCoordinateConfigValue(nameof(Dna2Location));
			Koh1Location = GetCoordinateConfigValue(nameof(Koh1Location));
			Koh2Location = GetCoordinateConfigValue(nameof(Koh2Location));
			Nadh1Location = GetCoordinateConfigValue(nameof(Nadh1Location));
			Nadh2Location = GetCoordinateConfigValue(nameof(Nadh2Location));
			Basal1Location = GetCoordinateConfigValue(nameof(Basal1Location));
			Basal2Location = GetCoordinateConfigValue(nameof(Basal2Location));
			BiopsyLocation = GetCoordinateConfigValue(nameof(BiopsyLocation));
			PlateOffset = GetCoordinateConfigValue(nameof(PlateOffset));
			DnaLaserLocation = GetCoordinateConfigValue(nameof(DnaLaserLocation));
			NadhLaserLocation = GetCoordinateConfigValue(nameof(NadhLaserLocation));
			WasteLocation = GetCoordinateConfigValue(nameof(WasteLocation));
			VerticalWellCount = GetConfigValue<int>(nameof(VerticalWellCount));
			HorizontalWellCount = GetConfigValue<int>(nameof(HorizontalWellCount));
			TestWellsPerPlate = GetConfigValue<int>(nameof(TestWellsPerPlate));
			WellOffset = GetCoordinateConfigValue(nameof(WellOffset));
			TraySterilizationTime = GetConfigValue<float>(nameof(TraySterilizationTime));
			BiospsySterilizationTime = GetConfigValue<float>(nameof(BiospsySterilizationTime));
			BiospsySpeed = GetConfigValue<float>(nameof(BiospsySpeed));
			BiopsyTravel = GetConfigValue<float>(nameof(BiopsyTravel));
			BiopsyZTravel = GetConfigValue<float>(nameof(BiopsyZTravel));
			BiospsyZSpeed = GetConfigValue<float>(nameof(BiospsyZSpeed));
			FluidPrimingVolume = GetConfigValue<int>(nameof(FluidPrimingVolume));
			BaselineProcessingDwell = GetConfigValue<int>(nameof(BaselineProcessingDwell));
			DnaVolume = GetConfigValue<int>(nameof(DnaVolume));
			NadhVolume = GetConfigValue<int>(nameof(NadhVolume));
			KohVolume = GetConfigValue<int>(nameof(KohVolume));
			OpticalSensorDwell = GetConfigValue<int>(nameof(OpticalSensorDwell));
			SensorReadDwell = GetConfigValue<int>(nameof(SensorReadDwell));
			Co2Dwell = GetConfigValue<int>(nameof(Co2Dwell));
			TestMediaAspirationVolume = GetConfigValue<int>(nameof(TestMediaAspirationVolume));
			StirringDistance = GetConfigValue<float>(nameof(StirringDistance));
			TestMediaTimerPeriod = GetConfigValue<int>(nameof(TestMediaTimerPeriod));
			KohTimerPeriod = GetConfigValue<int>(nameof(KohTimerPeriod));
			DnaTimerPeriod = GetConfigValue<int>(nameof(DnaTimerPeriod));
			KohAspirationVolume = GetConfigValue<int>(nameof(KohAspirationVolume));
			DnaAspirationVolume = GetConfigValue<int>(nameof(DnaAspirationVolume));
			DnaAspirationVolume = GetConfigValue<int>(nameof(DnaAspirationVolume));
			BypassDeviceCommands = GetConfigValue<int>(nameof(BypassDeviceCommands));
			AspirateKohSyringeVolumeMl = GetConfigValue<float>(nameof(AspirateKohSyringeVolumeMl));
			AspirateKohSyringeTravelMm = GetConfigValue<float>(nameof(AspirateKohSyringeTravelMm));
			AspirateSyringeSpeed = GetConfigValue<float>(nameof(AspirateSyringeSpeed));
			WellWidth = GetConfigValue<float>(nameof(WellWidth));
			WellDepth = GetConfigValue<float>(nameof(WellDepth));
			CleanUpAspirationVolume = GetConfigValue<int>(nameof(CleanUpAspirationVolume));
            BiopsyEjectHomePosition = GetConfigValue<int>(nameof(BiopsyEjectHomePosition));
            ComPortNumber = GetConfigValue<int>(nameof(ComPortNumber));
            BiospsyLoadingLocation = GetConfigValue<int>(nameof(BiospsyLoadingLocation));
            FirstWellLocation = GetCoordinateConfigValue(nameof(FirstWellLocation));
        }

		public int BypassDeviceCommands { get; set; } // zero for no, one for yes, more than one for a n millisecond pause when bypassing
		public bool TerminateOnSensorFailure { get; set; }
		public float FluidSyringeHomePosition { get; set; }
		public float AspirateFluidSyringeHomePosition { get; set; }
		public float AspirateKohSyringeHomePosition { get; set; }
		public float BiospsyLoadingLocation { get; set; } // position biopsy needle should be at for loading
        public float BiopsyEjectHomePosition { get; set; }
		public float WellWidth { get; set; } // mm of width of sample well
		public float WellDepth { get; set; } // mm of width of sample well
		public float PlateCapacity { get; set; } // number of sample plates this device can accomodate
		public float TrayTemperature { get; set; } // in Celcius, target temperature to heat the tray to
		public float FluidSyringeVolumeMl { get; set; }
		public float FluidSyringeLength { get; set; } // mm length of a fluid syringe's contents
		public float FluidSyringeSpeed { get; set; } // speed at which a fluid syringe should emitt its contents
		public float AspirateSyringeVolumeMl { get; set; }
		public float AspirateSyringeTravelMm { get; set; } // mm length of an aspirate syringe's contents
		public float AspirateKohSyringeVolumeMl { get; set; }
		public float AspirateKohSyringeTravelMm { get; set; } // mm length of a Koh aspirate syringe's contents
		public float AspirateSyringeSpeed { get; set; } // speed at which an aspirate syringe should withdraw its contents
		public float AspirateTopZTravel { get; set; } // total mm aspirate axes will descend/ascend to top of well
		public float AspirateStirSpeed { get; set; } // speed at which aspirate will move during stirring
		public Coordinate CameraLocation { get; set; }
		public Coordinate AspirateFluidLocation { get; set; }
		public Coordinate AspirateKohLocation { get; set; }
		public Coordinate Dna1Location { get; set; }
		public Coordinate Dna2Location { get; set; }
		public Coordinate Koh1Location { get; set; }
		public Coordinate Koh2Location { get; set; }
		public Coordinate Nadh1Location { get; set; }
		public Coordinate Nadh2Location { get; set; }
		public Coordinate Basal1Location { get; set; }
		public Coordinate Basal2Location { get; set; }
		public Coordinate BiopsyLocation { get; set; }
		public Coordinate PlateOffset { get; set; } // distance from well 0 of a tray to well 0 of the next tray
		public Coordinate DnaLaserLocation { get; set; }
		public Coordinate NadhLaserLocation { get; set; }
		public Coordinate WasteLocation { get; set; }
		public int VerticalWellCount { get; set; }
		public int HorizontalWellCount { get; set; }
		public int TestWellsPerPlate { get; set; } // total wells on a given plate to be used for testing
		public Coordinate WellOffset { get; set; } // distance between cells
		public float TraySterilizationTime { get; set; } // ammount of seconds tray will be UV sterilized
		public float BiospsySterilizationTime { get; set; } // ammount of seconds biopsy needle will be UV sterilized
		public float BiospsySpeed { get; set; } // speed at which the biopsy needle should descend/ascend
		public float BiopsyTravel { get; set; } // mm biopsy axis will descend/ascend
		public float BiopsyZTravel { get; set; } // mm biopsy Z axes will descend/ascend
		public float BiospsyZSpeed { get; set; } // speed at which the biopsy Z axis should descend/ascend
		public int FluidPrimingVolume { get; set; } // microliters dispensed during fluid pump priming
		public int KohVolume { get; set; } // microliters dispensed by KOH pump
		public int DnaVolume { get; set; } // microliters dispensed by DNA pump
		public int NadhVolume { get; set; } // microliters dispensed by NADH pump
		public int OpticalSensorDwell { get; set; } // total milliseconds to activate laser and read sensor
		public int SensorReadDwell { get; set; } // total milliseconds to wait for a sensor reading to be reported
		public int Co2Dwell { get; set; } // total milliseconds to activate CO2
		public int TestMediaAspirationVolume { get; set; } // microliters withdraw from baseline
		public float StirringDistance { get; set; } // total mm to move needle when stirring
		public int TestMediaTimerPeriod { get; set; } // seconds the test media timers will wait - 1h
		public int KohTimerPeriod { get; set; } // milliseconds the KOH timer will wait - 4hs
		public int DnaTimerPeriod { get; set; } // seconds the DNA timer will wait
		public int KohAspirationVolume { get; set; } // microliters withdraw for before KOH injection
		public int DnaAspirationVolume { get; set; } // microliters withdraw for before DNA reagent injection
		public int CleanUpAspirationVolume { get; set; } // microliters withdraw for clean up
		public int BaselineProcessingDwell { get; set; } // ammount of milliseconds baseline processing will wait
        public int ComPortNumber { get; set; }
        public Coordinate FirstWellLocation { get; set; }

        public DeviceSpecification? DeviceSpecification { get; private set; }
	}
}
