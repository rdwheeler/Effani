using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Configuration;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Drivetrain.Robotics.Control
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
			DeviceSpecification.Axes.Add(new TableAxis(Axis.TableX, 0, 0, 500, EndstopLocation.LowEnd, Direction.Backwards, TableAxisSpeed));
			DeviceSpecification.Axes.Add((AxisSpec)new TableAxis(Axis.TableY, 0, 1, 500, EndstopLocation.LowEnd, Direction.Backwards, TableAxisSpeed));
			DeviceSpecification.Axes.AddRange(GetAspirateAxes());
			DeviceSpecification.Axes.Add(new FluidAxis(Axis.DnaReagent1, 1, 4, DnaLocation, FluidSyringeLoadingPosition));
			DeviceSpecification.Axes.Add(new FluidAxis(Axis.Koh1, 2, 1, KohLocation, FluidSyringeLoadingPosition));
			DeviceSpecification.Axes.Add(new FluidAxis(Axis.Nadh1, 2, 3, NadhLocation, FluidSyringeLoadingPosition));
			var basal1 = new FluidAxis(Axis.Basal1, 0, 5, BasalLocation, FluidSyringeLoadingPosition);
            basal1.Driver.Endstop.SensorPort = ($"1.io0.in");
            DeviceSpecification.Axes.Add(basal1);

			DeviceSpecification.Sensors.Add(new DnaSensorSpec("DNA Sensor", DnaSensorNumber, DnaSensorBoardNumber, DnaSensorPortNumber, DnaSensorReadHigh, DnaSensorReadLow, DnaSensorLocation));
			DeviceSpecification.Sensors.Add(new NadhSensorSpec("NADH Sensor", NadhSensorNumber, NadhSensorBoardNumber, NadhSensorPortNumber, NadhSensorReadHigh, NadhSensorReadLow, NadhSensorLocation));

			DeviceSpecification.Plates.Add(new Plate(new Coordinate(0, 0)));
			DeviceSpecification.Plates.Add(new Plate(new Coordinate(1, 0)));
			Plate currentPlate = DeviceSpecification.Plates[0];
			var wellsPerPlate = (HorizontalWellCount * VerticalWellCount);
			for (int index = 0; index < 2 * wellsPerPlate; index++)
			{
				for (int x = 0; x < HorizontalWellCount; x++)
				{
					for (int y = 0; y < VerticalWellCount; y++)
					{
                        var baseX = FirstWellLocation.X + (PlateOffset.X * currentPlate.Location.X);
                        var baseY = FirstWellLocation.Y + (PlateOffset.Y * currentPlate.Location.Y);
						var location = new Coordinate(
							((x * WellOffset.X) + baseX), 
							((y * WellOffset.Y) + baseY));
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
			ret.Add(new AspirateZAxis(Axis.AspirateFluidZ, 0, 4, AspirateFluidLocation, AspirateFluidActuatorStartPosition));
            var aspirate1Z = new AspirateZAxis(Axis.AspirateKohZ, 1, 1, AspirateKohLocation, AspirateKohActuatorStartPosition);
            aspirate1Z.Driver.Endstop.SensorPort = ($"1.io1.in");
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
			FluidSyringeLoadingPosition = GetConfigValue<float>(nameof(FluidSyringeLoadingPosition));
			AspirateFluidActuatorStartPosition = GetConfigValue<float>(nameof(AspirateFluidActuatorStartPosition));
			AspirateKohActuatorStartPosition = GetConfigValue<float>(nameof(AspirateKohActuatorStartPosition));
			TrayTemperature = GetConfigValue<float>(nameof(TrayTemperature));
			FluidSyringeSpeed = GetConfigValue<float>(nameof(FluidSyringeSpeed));
			AspirateKohDistanceToPlate = GetConfigValue<float>(nameof(AspirateKohDistanceToPlate));
			AspirateFluidDistanceToPlate = GetConfigValue<float>(nameof(AspirateFluidDistanceToPlate));
			AspirateFluidLocation = GetCoordinateConfigValue(nameof(AspirateFluidLocation));
			AspirateKohLocation = GetCoordinateConfigValue(nameof(AspirateKohLocation));
			DnaLocation = GetCoordinateConfigValue(nameof(DnaLocation));
			KohLocation = GetCoordinateConfigValue(nameof(KohLocation));
			NadhLocation = GetCoordinateConfigValue(nameof(NadhLocation));
			BasalLocation = GetCoordinateConfigValue(nameof(BasalLocation));
			BiopsyLocation = GetCoordinateConfigValue(nameof(BiopsyLocation));
			PlateOffset = GetCoordinateConfigValue(nameof(PlateOffset));
			DnaSensorLocation = GetCoordinateConfigValue(nameof(DnaSensorLocation));
			DnaSensorBoardNumber = GetConfigValue<int>(nameof(DnaSensorBoardNumber));
			DnaSensorPortNumber = GetConfigValue<int>(nameof(DnaSensorPortNumber));
			DnaSensorNumber = GetConfigValue<int>(nameof(DnaSensorNumber));
			DnaSensorReadHigh = GetConfigValue<int>(nameof(DnaSensorReadHigh));
			DnaSensorReadLow = GetConfigValue<int>(nameof(DnaSensorReadLow));
			NadhSensorLocation = GetCoordinateConfigValue(nameof(NadhSensorLocation));
			NadhSensorBoardNumber = GetConfigValue<int>(nameof(NadhSensorBoardNumber));
			NadhSensorPortNumber = GetConfigValue<int>(nameof(NadhSensorPortNumber));
			NadhSensorNumber = GetConfigValue<int>(nameof(NadhSensorNumber));
			NadhSensorReadHigh = GetConfigValue<int>(nameof(NadhSensorReadHigh));
			NadhSensorReadLow = GetConfigValue<int>(nameof(NadhSensorReadLow));
			WasteLocation = GetCoordinateConfigValue(nameof(WasteLocation));
			VerticalWellCount = GetConfigValue<int>(nameof(VerticalWellCount));
			HorizontalWellCount = GetConfigValue<int>(nameof(HorizontalWellCount));
			WellOffset = GetCoordinateConfigValue(nameof(WellOffset));
			TraySterilizationTime = GetConfigValue<int>(nameof(TraySterilizationTime));
			InitialFluidPrimingVolume = GetConfigValue<int>(nameof(InitialFluidPrimingVolume));
			SecondaryFluidPrimingVolume = GetConfigValue<int>(nameof(SecondaryFluidPrimingVolume));
			StandardsProcessingDwell = GetConfigValue<int>(nameof(StandardsProcessingDwell));
			DnaVolume = GetConfigValue<int>(nameof(DnaVolume));
			NadhVolume = GetConfigValue<int>(nameof(NadhVolume));
			KohVolume = GetConfigValue<int>(nameof(KohVolume));
			SensorReadDwell = GetConfigValue<int>(nameof(SensorReadDwell));
			TestMediaAspirationVolume = GetConfigValue<int>(nameof(TestMediaAspirationVolume));
			NadhReadingsTimerPeriod = GetConfigValue<int>(nameof(NadhReadingsTimerPeriod));
			KohTimerPeriod = GetConfigValue<int>(nameof(KohTimerPeriod));
			DnaTimerPeriod = GetConfigValue<int>(nameof(DnaTimerPeriod));
			KohAspirationVolume = GetConfigValue<int>(nameof(KohAspirationVolume));
			DnaAspirationVolume = GetConfigValue<int>(nameof(DnaAspirationVolume));
			BypassDeviceCommands = GetConfigValue<int>(nameof(BypassDeviceCommands));
			AspiratePositioningSpeed = GetConfigValue<float>(nameof(AspiratePositioningSpeed));
			WellWidth = GetConfigValue<float>(nameof(WellWidth));
			WellDepth = GetConfigValue<float>(nameof(WellDepth));
			CleanUpAspirationVolume = GetConfigValue<int>(nameof(CleanUpAspirationVolume));
            ComPortNumber = GetConfigValue<int>(nameof(ComPortNumber));
            BasalVolume = GetConfigValue<int>(nameof(BasalVolume));
			StandardsWellCount = GetConfigValue<int>(nameof(StandardsWellCount));
			TimerDescriptionUpdateInterval = GetConfigValue<int>(nameof(TimerDescriptionUpdateInterval));
			TrayLoadingLocation = GetCoordinateConfigValue(nameof(TrayLoadingLocation));
			TraySterilizationLocation = GetCoordinateConfigValue(nameof(TraySterilizationLocation));
			FirstWellLocation = GetCoordinateConfigValue(nameof(FirstWellLocation));
			AspirateTravelPerMicroLiter = GetConfigValue<float>(nameof(AspirateTravelPerMicroLiter));
			FluidTravelPerMicroLiter = GetConfigValue<float>(nameof(FluidTravelPerMicroLiter));
			AspirateStirSpeed = GetConfigValue<float>(nameof(AspirateStirSpeed));
			TableAxisSpeed = GetConfigValue<int>(nameof(TableAxisSpeed));
		}

		public int BypassDeviceCommands { get; set; } // zero for no, one for yes, more than one for a n millisecond pause when bypassing
		public float FluidSyringeLoadingPosition { get; set; }
		public float AspirateFluidActuatorStartPosition { get; set; }
		public float AspirateKohActuatorStartPosition { get; set; }
		public float WellWidth { get; set; } // mm of width of sample well
		public float WellDepth { get; set; } // mm of width of sample well
		public float TrayTemperature { get; set; } // in Celcius, target temperature to heat the tray to
		public float FluidSyringeSpeed { get; set; } // speed at which a fluid syringe should emitt its contents
		public float AspirateKohDistanceToPlate { get; set; } // total mm aspirate KOH will descend/ascend to top of well
		public float AspirateFluidDistanceToPlate { get; set; } // total mm aspirate Fluid will descend/ascend to top of well
		public float AspiratePositioningSpeed { get; set; } // speed at which aspirate needles will move into position to begin stir
		public float AspirateStirSpeed { get; set; } // speed at which aspirate will move during stirring
		public Coordinate AspirateFluidLocation { get; set; }
		public Coordinate AspirateKohLocation { get; set; }
		public Coordinate DnaLocation { get; set; }
		public Coordinate KohLocation { get; set; }
		public Coordinate NadhLocation { get; set; }
		public Coordinate BasalLocation { get; set; }
		public Coordinate BiopsyLocation { get; set; }
		public Coordinate PlateOffset { get; set; } // distance from well 0 of a tray to well 0 of the next tray
		public Coordinate DnaSensorLocation { get; set; }
		public int DnaSensorBoardNumber { get; set; }
		public int DnaSensorPortNumber { get; set; }
		public int DnaSensorNumber { get; set; }
		public int DnaSensorReadHigh { get; set; }
		public int DnaSensorReadLow { get; set; }
		public Coordinate NadhSensorLocation { get; set; }
		public int NadhSensorBoardNumber { get; set; }
		public int NadhSensorPortNumber { get; set; }
		public int NadhSensorNumber { get; set; }
		public int NadhSensorReadHigh { get; set; }
		public int NadhSensorReadLow { get; set; }
		public Coordinate WasteLocation { get; set; }
		public int VerticalWellCount { get; set; }
		public int HorizontalWellCount { get; set; }
		public Coordinate WellOffset { get; set; } // distance between cells
		public int TraySterilizationTime { get; set; } // ammount of seconds tray will be UV sterilized
		public int InitialFluidPrimingVolume { get; set; } // microliters dispensed during fluid pump priming
		public int SecondaryFluidPrimingVolume { get; set; }
		public int KohVolume { get; set; } // microliters dispensed by KOH pump
		public int DnaVolume { get; set; } // microliters dispensed by DNA pump
		public int NadhVolume { get; set; } // microliters dispensed by NADH pump
        public int BasalVolume { get; set; } // microliters dispensed by Basal pump
        public int OpticalSensorDwell { get; set; } // total milliseconds to activate laser and read sensor
		public int SensorReadDwell { get; set; } // total milliseconds to wait for a sensor reading to be reported
		public int TestMediaAspirationVolume { get; set; } // microliters withdraw from baseline
		public float StirringDistance { get; set; } // total mm to move needle when stirring
		public int BasalIncubationTimerPeriod { get; set; } // miliseconds of the basal media incubation
		public int NadhReadingsTimerPeriod { get; set; } // miliseconds between NADH readings
		public int KohTimerPeriod { get; set; } // milliseconds the KOH timer will wait - 4hs
		public int DnaTimerPeriod { get; set; } // miliseconds the DNA timer will wait
		public int KohAspirationVolume { get; set; } // microliters to withdraw for before KOH injection
		public int DnaAspirationVolume { get; set; } // microliters to withdraw for before DNA reagent injection
		public int CleanUpAspirationVolume { get; set; } // microliters withdrawn for clean up
		public int StandardsProcessingDwell { get; set; } // milliseconds standards processing will wait
		public int TimerDescriptionUpdateInterval { get; set; } // milliseconds between updateing the UI with time-remaining descriptons
		public int ComPortNumber { get; set; }
		public int StandardsWellCount { get; set; }
		public Coordinate FirstWellLocation { get; set; }
        public Coordinate TrayLoadingLocation { get; set; }
		public Coordinate TraySterilizationLocation { get; set; }
		public float AspirateTravelPerMicroLiter { get; set; }
		public float FluidTravelPerMicroLiter { get; set; }
		public int TableAxisSpeed { get; set; }


		public DeviceSpecification? DeviceSpecification { get; private set; }
	}
}
