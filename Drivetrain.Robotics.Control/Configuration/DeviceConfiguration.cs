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
			DeviceSpecification = CreateDeviceSpec();
			AddPlates(DeviceSpecification);
		}
		public string GenerateConfigFiles(string drive)
		{
			try
			{
				var mainFolder = Path.Combine(drive, @"");
				var sysFolder = Path.Combine(mainFolder, @"sys");
				if(!Directory.Exists(sysFolder))
					Directory.CreateDirectory(sysFolder);
				// Build from the current settings: DeviceSpecification is only a snapshot taken when this object was created.
				var spec = CreateDeviceSpec();
				WriteConfig(sysFolder, spec);
				WriteHome(sysFolder, spec);
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
		/// <summary>
		/// Checks the configured board connections against each other and the installed hardware.
		/// Returns one message per problem found; an empty list means nothing suspicious was found.
		/// </summary>
		public List<string> GetConnectionWarnings()
		{
			var spec = CreateDeviceSpec();
			var warnings = new List<string>();
			var boardCount = Math.Max(1, Settings.ControllerBoardCount);
			var lastBoard = boardCount - 1;
			var boardNames = string.Join(" and ", Enumerable.Range(0, boardCount));

			void CheckBoard(string what, int board)
			{
				if (board < 0 || board > lastBoard)
					warnings.Add($"{what} is on board {board}, but only board{(boardCount == 1 ? "" : "s")} {boardNames} exist{(boardCount == 1 ? "s" : "")} ({boardCount} controller board{(boardCount == 1 ? "" : "s")} configured).");
			}
			void CheckDuplicates(string kind, IEnumerable<(string Label, string Name)> uses)
			{
				foreach (var group in uses.GroupBy(u => u.Name).Where(g => g.Count() > 1))
					warnings.Add($"{kind} {group.Key} is assigned to more than one item: {string.Join(", ", group.Select(g => g.Label))}.");
			}

			var axes = spec.Axes;
			foreach (var axis in axes)
			{
				CheckBoard($"{axis.Label} motor driver", axis.DriverBoardPort.Board);
				CheckBoard($"{axis.Label} endstop", axis.EndstopBoardPort.Board);
				if (axis.DriverBoardPort.Port < 0 || axis.DriverBoardPort.Port >= DriversPerBoard)
					warnings.Add($"{axis.Label} motor driver port {axis.DriverBoardPort.Port} does not exist; each board has drivers 0 to {DriversPerBoard - 1}.");
			}
			CheckDuplicates("Motor driver", axes.Select(a => (a.Label, a.DriverBoardPort.DriverDesignation)));

			var sensors = spec.Sensors;
			foreach (var sensor in sensors)
				CheckBoard(sensor.Label, sensor.Board);
			var inputCount = Settings.InputPortsPerBoard;
			void CheckInputPort(string what, BoardPort port)
			{
				if (port.Port < 0 || port.Port >= inputCount)
					warnings.Add($"{what} is on {port.InputDesignation}, but each board only has input ports 0 to {inputCount - 1} ({inputCount} input ports per board configured).");
			}
			foreach (var axis in axes)
				CheckInputPort($"{axis.Label} endstop", axis.EndstopBoardPort);

			CheckDuplicates("Input port", axes.Select(a => ($"{a.Label} endstop", a.EndstopBoardPort.InputDesignation)));
			CheckDuplicates("Temperature input", sensors.Select(s => (s.Label, s.PortDesignation!)));

			var outputs = GetActuatorOutputs();
			foreach (var output in outputs)
				CheckBoard(output.Label, output.Port.Board);
			CheckDuplicates("Output port", outputs.Select(o => (o.Label, o.Port.OutputDesignation)));

			return warnings;
		}
		private const int SuctionActuatorIndex = 5;
		/// <summary>Installed drivers per board (Duet 3 Mini 5+: drivers 0-4).</summary>
		private const int DriversPerBoard = 5;
		private List<(int Index, string Label, BoardPort Port)> GetActuatorOutputs() => new()
		{
			(0, "tray UV", new BoardPort(Settings.TrayUvBoardNumber, Settings.TrayUvPortNumber)),
			(1, "chamber UV", new BoardPort(Settings.ChamberUvBoardNumber, Settings.ChamberUvPortNumber)),
			(2, "NADH laser", new BoardPort(Settings.NadhLaserBoardNumber, Settings.NadhLaserPortNumber)),
			(3, "DNA laser", new BoardPort(Settings.DnaLaserBoardNumber, Settings.DnaLaserPortNumber)),
			(SuctionActuatorIndex, "aspiration suction", new BoardPort(Settings.AspirationSuctionToggleBoardNumber, Settings.AspirationSuctionTogglePortNumber)),
		};
		private void WriteConfig(string folder, DeviceSpecification spec)
		{
			var configTextBuilder = new StringBuilder();
			configTextBuilder.Append(@"
; General
G90 ; absolute coordinates
M83 ; relative extruder moves
M550 P""ABAL"" ; set hostname

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
			foreach (var x in spec.Axes)
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
			foreach (var x in spec.Axes)
			{
				configTextBuilder.Append($" {x.Designation}{x.DriverPort}");
			}
			configTextBuilder.Append($" ; set axis mapping\r\n");
			configTextBuilder.Append($"M350");
			foreach (var x in spec.Axes)
			{
				configTextBuilder.Append($" {x.Designation}16");
			}
			configTextBuilder.Append($" I1 ; configure microstepping with interpolation\r\n");
			configTextBuilder.Append($"M906");
			foreach (var x in spec.Axes)
			{
				configTextBuilder.Append($" {x.Designation}{x.Driver.Current}");
			}
			configTextBuilder.Append($" ; set axis driver currents\r\n");
			configTextBuilder.Append($"M92");
			foreach (var x in spec.Axes)
			{
				configTextBuilder.Append($" {x.Designation}{x.StepsPerMillimeter}");
			}
			configTextBuilder.Append($" ; configure steps per mm\r\n");
			configTextBuilder.Append($"M208");
			foreach (var x in spec.Axes)
			{
				configTextBuilder.Append($" {x.Designation}{x.Minimum}:{x.Maximum}");
			}
			configTextBuilder.Append($" ; set minimum and maximum axis limits\r\n");
			configTextBuilder.Append($"M566");
			foreach (var x in spec.Axes)
			{
				configTextBuilder.Append($" {x.Designation}900");
			}
			configTextBuilder.Append($" ; set maximum instantaneous speed changes (mm/min)\r\n");
			configTextBuilder.Append($"M203");
			foreach (var x in spec.Axes)
			{
				configTextBuilder.Append($" {x.Designation}{x.MaxSpeed}");
			}
			configTextBuilder.Append($" ; set maximum speeds (mm/min)\r\n");
			configTextBuilder.Append($"M201");
			foreach (var x in spec.Axes)
			{
				configTextBuilder.Append($" {x.Designation}500");
			}
			configTextBuilder.Append($" ; set accelerations (mm/s^2)\r\n");
			configTextBuilder.Append(@"

; Kinematics
M669 K0 ; configure Cartesian kinematics

; Endstops
");
            foreach (var x in spec.Axes)
            {
                configTextBuilder.AppendLine($"M574 {x.Designation}{(x.Driver.Endstop.Location == EndstopLocation.LowEnd ? "1" : "2")} P\"{(x.Driver.Endstop.Signal == PinSignal.High ? null : "!")}{x.Driver.Endstop.SensorPort}\" S1");
            }
            var t = configTextBuilder.ToString();
            configTextBuilder.AppendLine("\r\n; Actuators");
			foreach (var (index, label, port) in GetActuatorOutputs().Where(o => o.Index != SuctionActuatorIndex))
				configTextBuilder.AppendLine($@"M950 P{index} C""{port.OutputDesignation}"" ; {label}");
			var suction = GetActuatorOutputs().Single(o => o.Index == SuctionActuatorIndex);
			configTextBuilder.AppendLine(@$"M950 P{suction.Index} C""{suction.Port.OutputDesignation}"" ; aspiration emission switch");
			configTextBuilder.AppendLine("\r\n; Sensors");
			foreach (var sensor in spec.Sensors)
			{
				configTextBuilder.AppendLine($@"M308 S{sensor.SensorNumber} P""{sensor.PortDesignation}"" Y""linear-analog"" A""{sensor.Label}"" F0 B{sensor.ReadLow} C{sensor.ReadHigh}");
			}

			File.WriteAllBytes(Path.Combine(folder, "config.g"), Encoding.ASCII.GetBytes(configTextBuilder.ToString()));
		}
		private void WriteHome(string folder, DeviceSpecification spec)
		{
			for (int i = 0; i < spec.Axes.Count; i++)
			{
				var homeTextBuilder = new StringBuilder();
				homeTextBuilder.Append(@"
; Home
"
				);
				var x = spec.Axes[i];
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
			foreach (var x in spec.Axes)
			{
				homeAllBuilder.AppendLine($"M98 P\"home{x.Designation}.g\" ; home {x.Label} axis");
			}
			File.WriteAllBytes(Path.Combine(folder, $"homeall.g"), Encoding.ASCII.GetBytes(homeAllBuilder.ToString()));
		}

		/// <summary>Builds the axes and sensors from the current <see cref="Settings"/> values (plates are added separately by <see cref="AddPlates"/>).</summary>
		private DeviceSpecification CreateDeviceSpec()
		{
			var spec = new DeviceSpecification();
			spec.DeviceName = spec.Label;
			spec.Axes.Add(new TableAxis(Axis.TableX,
				new BoardPort(Settings.TableXDriverBoardNumber, Settings.TableXDriverPortNumber),
				new BoardPort(Settings.TableXEndstopBoardNumber, Settings.TableXEndstopPortNumber),
				500, EndstopLocation.LowEnd, Direction.Backwards, Settings.TableAxisSpeed));
			spec.Axes.Add(new TableAxis(Axis.TableY,
				new BoardPort(Settings.TableYDriverBoardNumber, Settings.TableYDriverPortNumber),
				new BoardPort(Settings.TableYEndstopBoardNumber, Settings.TableYEndstopPortNumber),
				500, EndstopLocation.LowEnd, Direction.Backwards, Settings.TableAxisSpeed));
			spec.Axes.AddRange(GetAspirateAxes());
			spec.Axes.Add(new FluidAxis(Axis.DnaReagent,
				new BoardPort(Settings.DnaReagentDriverBoardNumber, Settings.DnaReagentDriverPortNumber),
				new BoardPort(Settings.DnaReagentEndstopBoardNumber, Settings.DnaReagentEndstopPortNumber),
				Settings.DnaLocation, Settings.DnaSyringeLoadingPosition));
			spec.Axes.Add(new FluidAxis(Axis.Koh,
				new BoardPort(Settings.KohDriverBoardNumber, Settings.KohDriverPortNumber),
				new BoardPort(Settings.KohEndstopBoardNumber, Settings.KohEndstopPortNumber),
				Settings.KohLocation, Settings.KohSyringeLoadingPosition));
			spec.Axes.Add(new FluidAxis(Axis.Nadh,
				new BoardPort(Settings.NadhDriverBoardNumber, Settings.NadhDriverPortNumber),
				new BoardPort(Settings.NadhEndstopBoardNumber, Settings.NadhEndstopPortNumber),
				Settings.NadhLocation, Settings.NadhSyringeLoadingPosition));
			spec.Axes.Add(new FluidAxis(Axis.Basal,
				new BoardPort(Settings.BasalDriverBoardNumber, Settings.BasalDriverPortNumber),
				new BoardPort(Settings.BasalEndstopBoardNumber, Settings.BasalEndstopPortNumber),
				Settings.BasalLocation, Settings.BasalSyringeLoadingPosition));

			spec.Sensors.Add(new DnaSensorSpec("DNA Sensor", Settings.DnaSensorNumber, Settings.DnaSensorBoardNumber, Settings.DnaSensorPortNumber, Settings.DnaSensorReadHigh, Settings.DnaSensorReadLow, Settings.DnaSensorLocation));
			spec.Sensors.Add(new NadhSensorSpec("NADH Sensor", Settings.NadhSensorNumber, Settings.NadhSensorBoardNumber, Settings.NadhSensorPortNumber, Settings.NadhSensorReadHigh, Settings.NadhSensorReadLow, Settings.NadhSensorLocation));
			return spec;
		}
		private void AddPlates(DeviceSpecification spec)
		{
			spec.Plates.Add(new Plate(new Coordinate(0, 0)));
			spec.Plates.Add(new Plate(new Coordinate(1, 0)));
			Plate currentPlate = spec.Plates[0];
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
					if(currentPlate == spec.Plates.Last())
						break;

					currentPlate = spec.Plates
						.ElementAt(spec.Plates.IndexOf(currentPlate) + 1);
				}
			}
		}
		private List<AxisSpec> GetAspirateAxes()
		{
			var ret = new List<AxisSpec>();
			//ret.Add(new AspirateZAxis(Axis.AspirateFluid, 0, 4, Settings.AspirateFluidLocation, Settings.AspirateFluidActuatorStartPosition));
			ret.Add(new AspirateZAxis(Axis.Aspirate,
				new BoardPort(Settings.AspirateDriverBoardNumber, Settings.AspirateDriverPortNumber),
				new BoardPort(Settings.AspirateEndstopBoardNumber, Settings.AspirateEndstopPortNumber),
				Settings.AspirateLocation, Settings.AspirateActuatorStartPosition));

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
