using System.Collections.Concurrent;
using System.IO.Ports;
using System.Linq;

namespace Effani.Device.Control
{
	public partial class DeviceControl : IDisposable
	{
		public DeviceControl(string portName)
		{
			_serialPort = new SerialPort();
			_serialPort.PortName = portName; // SetPortName(_serialPort.PortName);
			_serialPort.BaudRate = 9600; // SetPortBaudRate(_serialPort.BaudRate);
			_serialPort.Parity = Parity.None; // SetPortParity(_serialPort.Parity);
			_serialPort.DataBits = 8; // SetPortDataBits(_serialPort.DataBits);
			_serialPort.StopBits = StopBits.One; // SetPortStopBits(_serialPort.StopBits);
			_serialPort.Handshake = Handshake.None; // SetPortHandshake(_serialPort.Handshake);
			_serialPort.ReadTimeout = 500;
			_serialPort.WriteTimeout = 500;
			_serialPort.DataReceived += _serialPort_DataReceived;

			Configuration = new DeviceConfiguration();
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
			var axisSpec = Axes[axis];
			if (!speed.HasValue)
				speed = axisSpec.MaxSpeed;
			Write("G91");
			Write($"G0 {axisSpec.Designation}{distance} F{speed}");
			return $"Move {axisSpec.Designation} {distance} at {speed}";
		}
		public string GoPercent(Axis axis, float percent, float? speed = null)
		{
			var axisSpec = Axes[axis];
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
			var axisSpec = Axes[axis];
			if (location < axisSpec.Minimum || location > axisSpec.Maximum)
				return "Location out of range";
			if (!speed.HasValue)
				speed = axisSpec.MaxSpeed;
			Write("G90");
			Write($"G0 {axisSpec.Designation}{location} F{speed}");
			return $"Move {axisSpec.Designation} to {location} at {speed}";
		}
		public string Home(Axis axis)
		{
			var axisSpec = Axes[axis];
			Write($"G28 {axisSpec.Designation}");
			return $"Homed {axisSpec.Label}";
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
			Write($"M122 B{boardId}");
			return "GetBoardDiagnostics";
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
				Write($"M308 S0");
				Task.Delay(Configuration.SensorReadDwell);
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
				SerialDataReceivedEventHandler dataRecieved = (sender, e) =>
				{
					recievedValue = _serialPort.ReadExisting();	
				};
				_serialPort.DataReceived += dataRecieved;
				Write($"M308 S1");
				Task.Delay(Configuration.SensorReadDwell);
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
			Write($"M42 P1 S1");
			return "Biopsy UV on";
		}
		public string BiopsyUvOff()
		{
			Write($"M42 P1 S0");
			return "Biopsy UV off";
		}
		public string LaserDnaOn()
		{
			Write($"M42 P2 S1");
			return "Laser DNA on";
		}
		public string LaserDnaOff()
		{
			Write($"M42 P2 S0");
			return "Laser DNA off";
		}
		public string LaserNadhOn()
		{
			Write($"M42 P3 S1");
			return "Laser NADH on";
		}
		public string LaserNadhOff()
		{
			Write($"M42 P3 S0");
			return "Laser NADH off";
		}
		public string EmitCo2(int miliseconds)
		{
			Write($"M42 P4 S1");
			Task.Delay(miliseconds);
			Write($"M42 P4 S0");
			return $"CO2 emitted for {miliseconds} miliseconds";
		}
		public string EmitFluid(Axis axis, int microLiters)
		{
			var axisSpec = Axes[axis];
			if(!(axisSpec is FluidAxis))
				return $"{axis} is not a fluid axis";
			var distance = CalcFluidTravelMmForMl(microLiters);
			var result = Move(axisSpec.Axis, distance, Configuration.FluidSyringeSpeed);
			if(!result.Equals(OK))
				return result;
			return $"{microLiters} {axis} emitted";
		}
		public string SetTrayTemperature(float temperature)
		{
			Write($"M140 S{temperature}");
			return $"Tray heat set to {temperature}C";
		}
		public string RemoveFluid(Axis axis, int microLiters)
		{
			var axisSpec = Axes[axis];
			if (!(axisSpec is AspirateAxis))
				return $"{axis} is not an aspirate axis";
			var distance = CalcAspirateTravelMmForMl(microLiters);
			var result = Move(axisSpec.Axis, distance, Configuration.AspirateSyringeSpeed);
			if (!result.Equals(OK))
				return result;
			return $"{axis} {microLiters} removed";
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
			var microLitersPerMm = Configuration.FluidSyringeVolumeMl / Configuration.FluidSyringeLength / 1000;
			var ret = microLitersPerMm * microLiters;
			return microLitersPerMm;
		}
		private float CalcAspirateTravelMmForMl(int microLiters)
		{
			var microLitersPerMm = Configuration.AspirateSyringeVolumeMl / Configuration.AspirateSyringeTravelMm / 1000;
			var ret = (microLitersPerMm * microLiters) * -1;
			return microLitersPerMm;
		}
		private void Write(string data)
		{
			if(!_serialPort.IsOpen)
				_serialPort.Open();
			_serialPort.WriteLine($"{data}");
		}
		private void PublishReadData()
		{
			while(ReadData.TryDequeue(out var message))
			{
				DataReceived?.Invoke(this, new DataArgs<string>(message));
			}
			Task.Delay(500);
		}

		public event EventHandler<DataArgs<string>>? DataReceived;

		public ConcurrentQueue<string> ReadData { get; set; } = new();
		public DeviceConfiguration? Configuration { get; private set; }
		public Dictionary<Axis, AxisSpec> Axes { get; private set; }

		public List<AxisSpec> FluidAxes => Axes.Values.Where(x => x is FluidAxis).ToList();

		SerialPort _serialPort;
		Thread _publishThread;
		bool _suspendSerialPortReading = false;
		const string OK = "ok";

	}
}
