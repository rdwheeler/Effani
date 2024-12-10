using System.Collections.Concurrent;
using System.IO.Ports;
using System.Linq;

namespace Effani.Device.Control
{
	public partial class DeviceControl : IDisposable
	{
		public DeviceControl()
		{
			Configuration = new DeviceConfiguration();
			_serialPort = new SerialPort();
			_serialPort.PortName = $"COM{Configuration.ComPortNumber}";
			_serialPort.BaudRate = 9600; // SetPortBaudRate(_serialPort.BaudRate);
			_serialPort.Parity = Parity.None; // SetPortParity(_serialPort.Parity);
			_serialPort.DataBits = 8; // SetPortDataBits(_serialPort.DataBits);
			_serialPort.StopBits = StopBits.One; // SetPortStopBits(_serialPort.StopBits);
			_serialPort.Handshake = Handshake.None; // SetPortHandshake(_serialPort.Handshake);
			_serialPort.ReadTimeout = 500;
			_serialPort.WriteTimeout = 500;
			_serialPort.DataReceived += _serialPort_DataReceived;

			Axes = Configuration.DeviceSpecification.Axes.ToDictionary(k => k.Axis, v => v);

			_publishThread = new Thread(PublishReadData);
			_publishThread.Start();
		}

		private void _serialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
		{
			if(_suspendSerialPortReading)
				return;

			string message = _serialPort.ReadExisting();
			if(!String.IsNullOrEmpty(message))
				ReadData.Enqueue(message);
        }
        public string Move(Axis axis, float distance, float? speed = null)
        {
            if (!Axes.TryGetValue(axis, out var axisSpec))
                return "Unknown axis";
            if (!speed.HasValue)
				speed = axisSpec.MaxSpeed;
			Write("G90");
			Write($"G0 {axisSpec.Designation}{distance} F{speed}");
			return $"Move {axisSpec.Designation} {distance} at {speed}";
		}

		public string MoveRelative(Axis axis, float distance, float? speed = null)
        {
            if (!Axes.TryGetValue(axis, out var axisSpec))
                return "Unknown axis";
            if (!speed.HasValue)
				speed = axisSpec.MaxSpeed;
			Write("G91");
			Write($"G0 {axisSpec.Designation}{distance} F{speed}");
			return $"Move {axisSpec.Designation} {distance} at {speed}";
		}
		public string Move(List<MoveSpec> moveSpecs, float speed)
		{
			var moveCode = String.Join(' ', moveSpecs.Select(s => s.ToGode()));
			Write("G90");
			Write($"G0 {moveCode} F{speed}");
			return $"Move {moveCode} at {speed}";
		}
		public string MoveClockwiseArc(List<MoveSpec> moveSpecs, float speed)
		{
			var moveCode = String.Join(' ', moveSpecs.Select(s => s.ToGode()));
			Write("G91");
            var formula = $"G2 {moveCode} R4.5 F{speed}";
            Write(formula);
			return $"Move {moveCode} at {speed}";
		}
		public string GoPercent(Axis axis, float percent, float? speed = null)
        {
            if (!Axes.TryGetValue(axis, out var axisSpec))
                return "Unknown axis";
            if (!speed.HasValue)
				speed = axisSpec.MaxSpeed;
			var location = (axisSpec.Maximum - axisSpec.Minimum) * (percent / 100);
			if(location < axisSpec.Minimum)
				location = axisSpec.Minimum;
			if (location > axisSpec.Maximum)
				location = axisSpec.Maximum;
			Write("G90");
			Write($"G0 {axisSpec.Designation}{location} F{speed}");
			return $"Move {axisSpec.Designation} to {location} ({percent}%) at {speed}";
		}
		public string GoTo(Axis axis, float location, float? speed = null)
        {
            if (!Axes.TryGetValue(axis, out var axisSpec))
                return "Unknown axis";
            if (location < axisSpec.Minimum || location > axisSpec.Maximum)
				return "Location out of range";
			if (!speed.HasValue)
				speed = axisSpec.MaxSpeed;
			Write("G90");
			Write($"G0 {axisSpec.Designation}{location} F{speed}");
			return $"Move {axisSpec.Designation} to {location} at {speed}";
		}
		public string TableGoTo(float x, float y, float? speed = null)
		{
			var xaxis = Axes[Axis.TableX];
			var yaxis = Axes[Axis.TableY];

			if (!speed.HasValue)
				speed = Math.Min(xaxis.MaxSpeed, yaxis.MaxSpeed);

			if (x < xaxis.Minimum || x > xaxis.Maximum ||
				y < yaxis.Minimum || y > yaxis.Maximum)
				return "Location out of range";

			Write("G90");
			Write($"G0 {xaxis.Designation}{x} {yaxis.Designation}{y} F{speed}");
			return $"Move table to {x}.{y} at {speed}";
		}
		public string TableGoTo(Coordinate location, float? speed = null)
		{
			return TableGoTo(location.X, location.Y, speed);
		}
		public string Home(Axis axis)
		{
            if (!Axes.TryGetValue(axis, out var axisSpec))
                return "Unknown axis";
			Write($"G28 {axisSpec.Designation}");
			return $"Homed {axisSpec.Label}";
		}
		public string Home(Axis[] axes)
		{
			var codes = String.Join(null, axes.Select(a => Axes[a].Designation));
			Write($"G28 {codes}");
			return $"Homed {codes}";
        }
        public string HomeAll()
        {
            Write($"G28");
            return $"Homed all";
        }
        public void ApplyConfig()
		{
			Write($"M98 P\"config.g\"");
		}
		public string SetWifi(string ssid, string password)
		{
			Write($"M552 S0");
			Write($"M587 S\"{ssid}\" P\"{password}\"");
			Write($"M552 S1");
			return "SetWifi";
		}
		public string GetBoardDiagnostics(int boardId)
        {
            try
            {
                _suspendSerialPortReading = true;
                string recievedValue = null;
                SerialDataReceivedEventHandler dataRecieved = (sender, e) =>
                {
                    recievedValue = _serialPort.ReadExisting();
                };
                _serialPort.DataReceived += dataRecieved;
                Write($"M122 B{boardId}");
                _serialPort.DataReceived -= dataRecieved;
                return recievedValue;
            }
            finally
            {
                _suspendSerialPortReading = false;
            }
        }
		public string ReadDna()
		{
			try
			{
				_suspendSerialPortReading = true;
				string recievedValue = null;
				SerialDataReceivedEventHandler dataRecieved = (sender, e) =>
				{
					recievedValue = _serialPort.ReadExisting();
				};
				_serialPort.DataReceived += dataRecieved;
                Write($"G4 P{Configuration.SensorReadDwell}");
				Write($"M308 S0");
				_serialPort.DataReceived -= dataRecieved;
				if (!String.IsNullOrEmpty(recievedValue))
				{
					try
					{
						var afterReading = recievedValue.Split("reading ")[1];
						var firstNumber = afterReading.Split(',')[0];
						if (float.TryParse(firstNumber, out var readingValue))
							recievedValue = readingValue.ToString();
					}
					catch
					{
						recievedValue = $"Could not parse response of: {recievedValue}";
					}
				}
				return recievedValue;
			}
			finally
			{
				_suspendSerialPortReading = false;
			}
		}
		public string ReadNadh()
		{
			try
			{
				_suspendSerialPortReading = true;
				string recievedValue = null;
                _ = _serialPort.ReadExisting(); // clear buffer
                SerialDataReceivedEventHandler dataRecieved = (sender, e) =>
				{
					recievedValue = _serialPort.ReadExisting();	
				};
				_serialPort.DataReceived += dataRecieved;
                Write($"G4 P{Configuration.SensorReadDwell}");
                //Write($"M409 K""sensors.analog[);
                Write($"M308 S1");
                var count = 720;
                while (recievedValue == null && count > 0)
                {
                    Task.Delay(1000).Wait();
                    count--;
                    if (!String.IsNullOrEmpty(recievedValue))
                    {
                        var afterReading = recievedValue.Split("reading ")[1];
                        var firstNumber = afterReading.Split(',')[0];
                        if (float.TryParse(firstNumber, out var readingValue))
                        {
                            recievedValue = readingValue.ToString();
                            return recievedValue;
                        }
                    }
                }
                _serialPort.DataReceived -= dataRecieved;
				return recievedValue;
			}
			finally
			{
				_suspendSerialPortReading = false;
			}
		}

		public string PlateUvOn()
		{
			Write($"M42 P0 S1");
			return "Plate UV on";
		}
		public string PlateUvOff()
		{
			Write($"M42 P0 S0");
			return "Plate UV off";
		}
		public string BiopsyUvOn()
		{
			Write($"M42 P1 S0");
			return "Biopsy UV on";
        }
        public string BiopsyUvOff()
		{
			Write($"M42 P1 S1"); // needle UV uses voltage to signal off state because its a dimmer
            return "Biopsy UV off";
		}
		public string LaserDnaOn()
		{
			Write($"M42 P3 S1");
			return "Laser DNA on";
		}
		public string LaserDnaOff()
		{
			Write($"M42 P3 S0");
			return "Laser DNA off";
		}
		public string LaserNadhOn()
		{
			Write($"M42 P2 S1");
			return "Laser NADH on";
		}
		public string LaserNadhOff()
		{
			Write($"M42 P2 S0");
			return "Laser NADH off";
		}
		public string EmitCo2(int miliseconds)
        {
            Write($"G4 P1000");
            Write($"M42 P4 S1");
            Write($"G4 P{miliseconds}");
            Write($"M42 P4 S0");
			return $"CO2 emitted for {miliseconds} miliseconds";
		}
		public string EmitFluid(Axis axis, int microLiters)
        {
            if (!Axes.TryGetValue(axis, out var axisSpec))
                return "Unknown axis";
            if (!(axisSpec is FluidAxis))
				return $"{axis} is not a fluid axis";
			var distance = CalcFluidTravelMmForMl(microLiters);
			var result = MoveRelative(axisSpec.Axis, distance, Configuration.FluidSyringeSpeed);
            Write($"G4 P5000");
            if (!result.Equals(OK))
				return result;
			return $"{microLiters} {axis} emitted";
		}
		public string SetTrayTemperature(float temperature)
		{
			Write($"M140 S{temperature}");
			return $"Tray heat set to {temperature}C";
		}
		public string RemoveFluid(int microLiters)
		{
			Move(Axis.AspirateFluidZ, Configuration.AspirateTopZTravel, 10);
			DoStir(Axes[Axis.AspirateFluidZ]);

			var distance = CalcFluidAspirateTravelMmForMl(microLiters);
            MoveRelative(Axis.AspirateFluid, distance, Configuration.AspirateSyringeSpeed);
            Write($"G4 P5000");
			Move(Axis.AspirateFluidZ, Configuration.AspirateTopZTravel * -1, 10);

            return $"{microLiters} of fluid removed";
		}
		private void DoStir(AxisSpec axis)
		{
			MoveRelative(Axis.TableY, Configuration.WellWidth * 0.8f, 50); // move near well edge
            MoveRelative(axis.Axis, Configuration.WellDepth * 0.8f, 50); // move near well edge
   //         int count = 0;
			//while(count <= 5)
			//{
			//	//arc to right edge
			//	var rightMultiAxis = new List<MoveSpec>();
			//	rightMultiAxis.Add(new MoveSpec(Axes[Axis.TableX], (Configuration.WellWidth * 0.9f) * -1));
			//	rightMultiAxis.Add(new MoveSpec(Axes[Axis.TableY], (Configuration.WellWidth * 0.9f) * -1));
			//	rightMultiAxis.Add(new MoveSpec(axis, (Configuration.WellDepth * 0.9f)));
			//	MoveClockwiseArc(rightMultiAxis, Configuration.AspirateStirSpeed);
			//	//arc to bottom edge
			//	var bottomMultiAxis = new List<MoveSpec>();
			//	bottomMultiAxis.Add(new MoveSpec(Axes[Axis.TableX], (Configuration.WellWidth * 0.9f)));
			//	bottomMultiAxis.Add(new MoveSpec(Axes[Axis.TableY], (Configuration.WellWidth * 0.9f) * -1));
			//	bottomMultiAxis.Add(new MoveSpec(axis, (Configuration.WellDepth * 0.9f)));
			//	MoveClockwiseArc(rightMultiAxis, Configuration.AspirateStirSpeed);
			//	//arc to left edge
			//	var leftMultiAxis = new List<MoveSpec>();
			//	leftMultiAxis.Add(new MoveSpec(Axes[Axis.TableX], (Configuration.WellWidth * 0.9f)));
			//	leftMultiAxis.Add(new MoveSpec(Axes[Axis.TableY], (Configuration.WellWidth * 0.9f)));
			//	leftMultiAxis.Add(new MoveSpec(axis, (Configuration.WellDepth * 0.2f)));
			//	MoveClockwiseArc(rightMultiAxis, Configuration.AspirateStirSpeed);
			//	//arc to top edge
			//	var topMultiAxis = new List<MoveSpec>();
			//	topMultiAxis.Add(new MoveSpec(Axes[Axis.TableX], (Configuration.WellWidth * 0.9f) * -1));
			//	topMultiAxis.Add(new MoveSpec(Axes[Axis.TableY], (Configuration.WellWidth * 0.9f) * -1));
			//	topMultiAxis.Add(new MoveSpec(axis, (Configuration.WellDepth * 0.9f)));
			//	MoveClockwiseArc(rightMultiAxis, Configuration.AspirateStirSpeed);
			//	count++;
			//}
		}
		public string RemoveKoh(int microLiters)
		{
			Move(Axis.AspirateKohZ, Configuration.AspirateTopZTravel, 10);
			DoStir(Axes[Axis.AspirateKohZ]);

			var distance = CalcKohAspirateTravelMmForMl(microLiters);
			MoveRelative(Axis.AspirateKoh, distance, Configuration.AspirateSyringeSpeed);
            Write($"G4 P5000");
			Move(Axis.AspirateFluidZ, Configuration.AspirateTopZTravel * -1, 10);

            return $"{microLiters} of KOH removed";
		}
		public void Dispose()
		{
			_serialPort.Close();
			if(_publishThread is not null)
			{
				_publishThread.Join();
				_publishThread = null;
			}
		}

		private float CalcFluidTravelMmForMl(int microLiters)
		{
			var microLitersPerMm = Configuration.FluidSyringeVolumeMl / Configuration.FluidSyringeLength / 1000 * Configuration.FluidDispensationAdjustmentFactor;
			var ret = microLitersPerMm * microLiters;
			return ret;
		}
		private float CalcFluidAspirateTravelMmForMl(int microLiters)
		{
			var microLitersPerMm = Configuration.AspirateSyringeVolumeMl / Configuration.AspirateSyringeTravelMm / 1000;
			var ret = (microLitersPerMm * microLiters) * -1;
			return ret;
		}
		private float CalcKohAspirateTravelMmForMl(int microLiters)
		{
			var microLitersPerMm = Configuration.AspirateKohSyringeVolumeMl / Configuration.AspirateKohSyringeTravelMm / 1000;
			var ret = (microLitersPerMm * microLiters) * -1;
			return ret;
		}
		private void Write(string data)
		{
			if(!_serialPort.IsOpen)
				_serialPort.Open();
            _serialPort.WriteLine($"M400");
            _serialPort.WriteLine($"{data}");
		}
		private void PublishReadData()
		{
			while(ReadData.TryDequeue(out var message))
			{
				DataReceived?.Invoke(this, new DataArgs<string>(message));
			}
			Task.Delay(500).Wait();
		}

		public event EventHandler<DataArgs<string>>? DataReceived;

		public ConcurrentQueue<string> ReadData { get; set; } = new();
		public DeviceConfiguration? Configuration { get; private set; }
		public Dictionary<Axis, AxisSpec> Axes { get; private set; }

		public List<AxisSpec> FluidAxes => Axes.Values.Where(x => x is FluidAxis).ToList();
        public List<AxisSpec> ZAxes => Axes.Values.Where(x => x is BiopsyZAxis).ToList();

        SerialPort _serialPort;
		Thread _publishThread;
		bool _suspendSerialPortReading = false;
		const string OK = "ok";

	}
}
