using System.IO.Ports;
using System.Linq;

namespace Effani.Device.Control
{
	public partial class Device : IDisposable
	{
		public Device(string portName)
		{
			// Create a new SerialPort object with default settings.
			_serialPort = new SerialPort();

			// Allow the user to set the appropriate properties.
			_serialPort.PortName = portName; // SetPortName(_serialPort.PortName);
			_serialPort.BaudRate = 9600; // SetPortBaudRate(_serialPort.BaudRate);
			_serialPort.Parity = Parity.None; // SetPortParity(_serialPort.Parity);
			_serialPort.DataBits = 8; // SetPortDataBits(_serialPort.DataBits);
			_serialPort.StopBits = StopBits.One; // SetPortStopBits(_serialPort.StopBits);
			_serialPort.Handshake = Handshake.None; // SetPortHandshake(_serialPort.Handshake);

			// Set the read/write timeouts
			_serialPort.ReadTimeout = 500;
			_serialPort.WriteTimeout = 500;
			
			_deviceConfiguration = new DeviceConfiguration();
			_spec = _deviceConfiguration.GetDeviceSpecification();
			_axes = _spec.Axes.ToDictionary(k => k.Axis, v => v);

			//_readThread = new Thread(Read);
			//_continue = true;
			//_readThread.Start();
		}

		public string Move(AxisSpec axisSpec, float distance, float? speed = null)
		{
			if(!speed.HasValue)
				speed = axisSpec.MaxSpeed;
			var cmd = $"G0 {axisSpec.Designation}{distance} F{speed}";
			Write("G91");
			var result = Write(cmd);
			return result;
		}
		public string GoPercent(AxisSpec axisSpec, float percent, float? speed = null)
		{
			if (!speed.HasValue)
				speed = axisSpec.MaxSpeed;
			var location = (axisSpec.Maximum - axisSpec.Minimum) * (percent / 100);
			if(location < axisSpec.Minimum)
				location = axisSpec.Minimum;
			if (location > axisSpec.Maximum)
				location = axisSpec.Maximum;
			Write("G90");
			var result = Write($"G0 {axisSpec.Designation}{location} F{speed}");
			return result;
		}
		public string GoTo(AxisSpec axisSpec, float location, float? speed = null)
		{
			if(location < axisSpec.Minimum || location > axisSpec.Maximum)
				return "Location out of range";
			if (!speed.HasValue)
				speed = axisSpec.MaxSpeed;
			Write("G90");
			var result = Write($"G0 {axisSpec.Designation}{location} F{speed}");
			return result;
		}
		public string Home(AxisSpec axisSpec)
		{
			Write($"G28 {axisSpec.Designation}");
			return $"Homed {axisSpec.Label}";
		}
		public void ApplyConfig()
		{
			Write($"M98 P\"config.g\"");
		}
		public List<string> SetWifi(string ssid, string password)
		{
			Write($"M552 S0");
			Write($"M587 S\"{ssid}\" P\"{password}\"");
			Write($"M552 S1");
			return ReadMany(10);
		}
		public List<string> GetBoardDiagnostics(int boardId)
		{
			Write($"M122 B{boardId}");
			return ReadMany(3);
		}
		public List<string> ReadDna()
		{
			Write($"M308 S0");
			return ReadMany(10);
		}
		public List<string> ReadNadh()
		{
			Write($"M308 S1");
			return ReadMany(3);
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
			var axisSpec = _axes[axis];
			if(!(axisSpec.Driver is FluidAxis))
				return $"{axis} is not a fluid axis";
			var distance = CalcFluidTravelMmForMl(microLiters);
			var result = Move(axisSpec, distance, _deviceConfiguration.FluidSyringeSpeed);
			if(!result.Equals(OK))
				return result;
			return $"{microLiters} {axis} emitted";
		}
		public string RemoveFluid(Axis axis, int microLiters)
		{
			var axisSpec = _axes[axis];
			if (!(axisSpec.Driver is AspirateAxis))
				return $"{axis} is not an aspirate axis";
			var distance = CalcAspirateTravelMmForMl(microLiters);
			var result = Move(axisSpec, distance, _deviceConfiguration.AspirateSyringeSpeed);
			if (!result.Equals(OK))
				return result;
			return $"{axis} {microLiters} removed";
		}
		public void Dispose()
		{
			_serialPort.Close();
			_readThread.Join();
		}

		private float CalcFluidTravelMmForMl(int microLiters)
		{
			var microLitersPerMm = _deviceConfiguration.FluidSyringeVolumeMl / _deviceConfiguration.FluidSyringeTravelMm / 1000;
			var ret = microLitersPerMm * microLiters;
			return microLitersPerMm;
		}
		private float CalcAspirateTravelMmForMl(int microLiters)
		{
			var microLitersPerMm = _deviceConfiguration.AspirateSyringeVolumeMl / _deviceConfiguration.AspirateSyringeTravelMm / 1000;
			var ret = (microLitersPerMm * microLiters) * -1;
			return microLitersPerMm;
		}
		private string Write(string data)
		{
			if(!_serialPort.IsOpen)
				_serialPort.Open();
			_serialPort.Write($"{data}\n");
			return ReadOne();
		}
		private void Read()
		{
			while (_continue)
			{
				try
				{
					Task.Delay(500);
					string message = _serialPort.ReadLine();
					if (!String.IsNullOrEmpty(message))
						DataReceived?.Invoke(this, new DataArgs<string>(message));
				}
				catch (TimeoutException ex) 
				{
					var a = ex;
				}
				catch (OperationCanceledException ex)
				{
					var a = ex;
				}
				catch (InvalidOperationException ex)
				{
					var a = ex;
				}
			}
		}
		private List<string> ReadMany(int seconds)
		{
			var ret = new List<string>();
			for (int attempt = 0; attempt <= seconds * 4; attempt++)
			{
				try
				{
					string message = _serialPort.ReadLine();
					if (!String.IsNullOrEmpty(message))
						ret.Add(message);
					Task.Delay(250);
				}
				catch (TimeoutException ex) { }
				catch (OperationCanceledException ex)
				{
					ret.Add("Canceled read");
				}
				catch (InvalidOperationException ex)
				{
					ret.Add("Port closed");
				}
			}
			if (!ret.Any())
				ret.Add("No message recieved");
			return ret;
		}
		private string ReadOne(int? timeout = null)
		{
			if(!timeout.HasValue)
				timeout = 60;
			for (int attempt = 0; attempt <= timeout * 4; attempt++)
			{
				try
				{
					string message = _serialPort.ReadLine();
					if(!String.IsNullOrEmpty(message))
						return message;
					Task.Delay(250);
				}
				catch (TimeoutException ex) { }
				catch (OperationCanceledException ex)
				{
					return "Canceled read";
				}
				catch (InvalidOperationException ex)
				{
					return "Port closed";
				}
			}
			
			return "No message recieved";
		}

		SerialPort _serialPort;
		static bool _continue;
		DeviceSpecification? _spec;
		Thread _readThread;
		DeviceConfiguration? _deviceConfiguration;
		Dictionary<Axis, AxisSpec> _axes;
		const string OK = "ok";

		public event EventHandler<DataArgs<string>> DataReceived;
	}
}
