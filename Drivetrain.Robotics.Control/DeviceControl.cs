using System.Collections.Concurrent;
using System.IO.Ports;
using System.Linq;
using System.Management;
using System.Text;

namespace Drivetrain.Robotics.Control
{
	public class DeviceControl : IDisposable
	{
		#region Constructors
		public DeviceControl()
		{
			Configuration = new DeviceConfiguration();
			_serialPort = new SerialPort();
			_serialPort.BaudRate = 9600; // SetPortBaudRate(_serialPort.BaudRate);
			_serialPort.Parity = Parity.None; // SetPortParity(_serialPort.Parity);
			_serialPort.DataBits = 8; // SetPortDataBits(_serialPort.DataBits);
			_serialPort.StopBits = StopBits.One; // SetPortStopBits(_serialPort.StopBits);
			_serialPort.Handshake = Handshake.None; // SetPortHandshake(_serialPort.Handshake);
			_serialPort.ReadTimeout = 500;
			_serialPort.WriteTimeout = 500;
			using (var searcher = new ManagementObjectSearcher("SELECT * FROM WIN32_SerialPort"))
			{
				string[] portnames = SerialPort.GetPortNames();
				var ports = searcher.Get().Cast<ManagementBaseObject>().ToList();
				foreach (var port in ports)
				{
					if (port["Caption"].ToString().Contains("duet", StringComparison.OrdinalIgnoreCase))
					{
						_serialPort.PortName = port["DeviceID"].ToString();
						break;
					}
				}
				if (String.IsNullOrEmpty(_serialPort.PortName))
					_serialPort.PortName = $"COM{Configuration.ComPortNumber}";
			}

			_publishThread = new Thread(InitializeReadData);
			_publishThread.Start();
		}
		#endregion

		#region Public methods
		public void AccOff(int accDesignation)
		{
			WriteWhenReady($"M42 P{accDesignation} S0");
		}
		public void AccOn(int accDesignation)
		{
			WriteWhenReady($"M42 P{accDesignation} S1");
		}
		public void ApplyConfig()
		{
			WriteWhenReady($"M98 P\"config.g\"");
		}
		public void Dispose()
		{
			_serialPort.Close();
			if (_publishThread is not null)
			{
				_publishThread.Join();
				_publishThread = null;
			}
		}
		public void GetBoardDiagnostics(int boardId)
		{
			WriteWhenReady($"M122 B{boardId}");
		}
		public void SendCommand(string command)
		{
			WriteNow(command);
		}
		public void Home(AxisSpec axisSpec)
		{
			WriteWhenReady($"G28 {axisSpec.Designation}");
		}
		public void HomeAll()
		{
			WriteWhenReady($"G28");
		}
		public void Move(List<MoveSpec> moveSpecs, float speed)
		{
			var moveCode = String.Join(' ', moveSpecs.Select(s => s.ToGode()));
			WriteWhenReady("G90");
			WriteWhenReady($"G0 {moveCode} F{speed}");
		}
		public void MoveAbsolute(List<MoveSpec> moveSpecs, float speed)
		{
			var moveCode = String.Join(' ', moveSpecs.Select(s => s.ToGode()));
			WriteWhenReady("G90");
			var formula = $"G0 {moveCode} F{speed}";
			WriteWhenReady(formula);
		}
		public void MoveAbsolute(AxisSpec axisSpec, float location, float? speed = null)
		{
			if (location < axisSpec.Minimum || location > axisSpec.Maximum)
				return;

			if (!speed.HasValue)
				speed = axisSpec.MaxSpeed;
			WriteWhenReady("G90");
			WriteWhenReady($"G0 {axisSpec.Designation}{location} F{speed}");
		}
		public void MoveToStarting(AxisSpec axisSpec)
		{
			var speed = axisSpec.MaxSpeed;
			WriteWhenReady("G90");
			WriteWhenReady($"G0 {axisSpec.Designation}{axisSpec.StartingPosition} F{speed}");
		}
		public void MoveRelativeClockwiseArc(List<MoveSpec> moveSpecs, float speed)
		{
			var moveCode = String.Join(' ', moveSpecs.Select(s => s.ToGode()));
			WriteWhenReady("G91");
			var formula = $"G2 {moveCode} R4.5 F{speed}";
			WriteWhenReady(formula);
		}
		public void MoveRelativeCounterClockwiseArc(List<MoveSpec> moveSpecs, float speed)
		{
			var moveCode = String.Join(' ', moveSpecs.Select(s => s.ToGode()));
			WriteWhenReady("G91");
			var formula = $"G3 {moveCode} R4.5 F{speed}";
			WriteWhenReady(formula);
		}
		public void MovePercent(AxisSpec axisSpec, float percent, float? speed = null)
		{
			if (!speed.HasValue)
				speed = axisSpec.MaxSpeed;
			var location = (axisSpec.Maximum - axisSpec.Minimum) * (percent / 100);
			if (location < axisSpec.Minimum)
				location = axisSpec.Minimum;
			if (location > axisSpec.Maximum)
				location = axisSpec.Maximum;

			WriteWhenReady("G90");
			WriteWhenReady($"G0 {axisSpec.Designation}{location} F{speed}");
		}
		public void MoveRelative(AxisSpec axisSpec, float distance, float? speed = null)
		{
			if (!speed.HasValue)
				speed = axisSpec.MaxSpeed;

			WriteWhenReady("G91");
			WriteWhenReady($"G0 {axisSpec.Designation}{distance} F{speed}");
		}
		public void Pause(int miliseconds)
		{
			WriteWhenReady($"G4 P{miliseconds}");
		}
		public string ReadSensor(int sensorDesignation, int pause)
		{
			if(pause > 0)
				Pause(pause);
			var response = WriteWhenReady($"M308 S{sensorDesignation}", STR_sensorKey);
			var first = response.Split("reading ")[1];
			var last = first.Split(",")[0];
			return last;
		}
		public void SetTrayTemperature(float temperature)
		{
			WriteWhenReady($"M140 S{temperature}");
		}
		public void SetWifi(string ssid, string password)
		{
			WriteWhenReady($"M552 S0");
			WriteWhenReady($"M587 S\"{ssid}\" P\"{password}\"");
			WriteWhenReady($"M552 S1");
		}
		#endregion

		#region Private methods
		private string GetResponse(string responseKey = null)
		{
			var ret = new StringBuilder();

			string firstResponse = null;
			while (firstResponse == null)
			{
				if (_responses.TryDequeue(out var response) && response.Contains(responseKey))
				{
					firstResponse = response.Substring(response.IndexOf(response));
					break;
				}
				Task.Delay(50).Wait();
			}
			ret.Append(firstResponse);
			while (_responses.TryDequeue(out var response))
			{
				ret.Append(response);
				Task.Delay(500).Wait();
			}
			_saveResponses = false;

			return ret.ToString();
		}
		private void InitializeReadData()
		{
			_serialPort.DataReceived += SerialPort_DataReceived;
			PublishReadData();
		}
		private void PublishReadData()
		{
			while(true)
			{
				while(ReadData.TryDequeue(out var message))
				{
					DataReceived?.Invoke(this, new DataArgs<string>(message));
				}
				Task.Delay(500).Wait();
			}
		}
		private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
		{
			string data = _serialPort.ReadExisting();
			if(String.IsNullOrEmpty(data))
				return;

			data = data.Trim('\r', '\n');
			var messages = data.Split("\n");
			foreach(var message in messages)
			{
				if(!String.IsNullOrEmpty(message))
				{
					if(message == STR_ready)
						_readyFound = true;
					else if(_saveResponses && message != "ok")
						_responses.Enqueue(message);
					else if(message != "ok")
						ReadData.Enqueue(message);
				}
			}
		}
		private void WaitForReady()
		{
			_serialPort.WriteLine($"M400");
			_readyFound = false;
			_serialPort.WriteLine($"echo \"{STR_ready}\"");
			while (!_readyFound)
			{
				Task.Delay(50).Wait();
			}
		}
		private void WriteNow(string data, string responseKey = null)
		{
			if (!_serialPort.IsOpen)
				_serialPort.Open();

			_serialPort.WriteLine($"{data}");
		}
		private string? WriteWhenReady(string data, string responseKey = null)
		{
			try
			{
				if (!_serialPort.IsOpen)
					_serialPort.Open();

				if (responseKey == null)
				{
					_serialPort.WriteLine($"{data}");
					WaitForReady();
					return null;
				}
				else
				{
					_saveResponses = true;
					_serialPort.WriteLine($"{data}");
					var ret = GetResponse(responseKey);
					WaitForReady();
					return ret;
				}
			}
			catch (Exception ex)
			{
				return ex.Message;
			}
		}
		#endregion

		#region Properties
		public DeviceConfiguration? Configuration { get; private set; }
		public ConcurrentQueue<string> ReadData { get; set; } = new();
		#endregion

		#region Events
		public event EventHandler<DataArgs<string>>? DataReceived;
		#endregion

		#region Fields
		Thread _publishThread;
		ConcurrentQueue<string> _responses = new();
		SerialPort _serialPort;
		public bool _saveResponses;

		private const string STR_sensorKey = "Sensor";
		private const string STR_ready = "ready";
		private bool _readyFound;
		#endregion
	}
}
