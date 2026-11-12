using Serilog;
using System.Collections.Concurrent;
using System.IO.Ports;
using System.Linq;
using System.Text;

namespace Drivetrain.Robotics.Control
{
	public class DeviceControl : IDisposable
	{
		#region Constructors
		/// <param name="settingsProvider">
		/// Returns the settings currently in effect. It is called whenever the <see cref="Configuration"/> has to be (re)built, so
		/// settings that were changed, activated or reset in the meantime are picked up (see <see cref="ResetConfiguration"/>).
		/// </param>
		public DeviceControl(Func<Settings> settingsProvider)
		{
			_settingsProvider = settingsProvider;
		}
		#endregion

		#region Public methods
		/// <summary>
		/// Creates the serial port object and starts the thread that publishes data read from it. Safe to call more than once;
		/// it is also called by the first operation that needs the port, so calling it explicitly is only needed to start
		/// publishing <see cref="DataReceived"/> early. Does not open the port.
		/// </summary>
		public void Initialize()
		{
			lock (_initializeLock)
			{
				if (_initialized)
					return;

				var port = new SerialPort();
				port.BaudRate = 9600; // SetPortBaudRate(port.BaudRate);
				port.Parity = Parity.None; // SetPortParity(port.Parity);
				port.DataBits = 8; // SetPortDataBits(port.DataBits);
				port.StopBits = StopBits.One; // SetPortStopBits(port.StopBits);
				port.Handshake = Handshake.None; // SetPortHandshake(port.Handshake);
				port.ReadTimeout = 500;
				port.WriteTimeout = 500;
				_serialPort = port;
				_publishThread = new Thread(InitializeReadData);
				_publishThread.Start();
				_initialized = true;
			}
		}
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
		public virtual void Dispose()
		{
			_disposing = true;
			if (_initialized)
				_serialPort.Close();
		}
		/// <summary>
		/// Discards the device configuration so that it is rebuilt from the current settings the next time it is needed.
		/// Call this only while nothing is running (no assay, empty process queue): anything holding axis specifications
		/// from the old configuration keeps using the old values.
		/// </summary>
		public void ResetConfiguration()
		{
			lock (_configurationLock)
			{
				_configuration = null;
				OnConfigurationReset();
			}
		}
		/// <summary>Called, with the configuration lock held, when the configuration is discarded; derived classes drop what they built from it.</summary>
		protected virtual void OnConfigurationReset() { }
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
		public void Move(List<MoveSpec> moveSpecs, decimal speed)
		{
			var moveCode = String.Join(' ', moveSpecs.Select(s => s.ToGode()));
			WriteWhenReady("G90");
			WriteWhenReady($"G0 {moveCode} F{speed}");
		}
		public void MoveAbsolute(List<MoveSpec> moveSpecs, decimal speed)
		{
			var moveCode = String.Join(' ', moveSpecs.Select(s => s.ToGode()));
			WriteWhenReady("G90");
			var formula = $"G0 {moveCode} F{speed}";
			WriteWhenReady(formula);
		}
		public void MoveAbsolute(AxisSpec axisSpec, decimal location, decimal? speed = null)
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
		public void MoveRelativeClockwiseArc(List<MoveSpec> moveSpecs, decimal speed)
		{
			var moveCode = String.Join(' ', moveSpecs.Select(s => s.ToGode()));
			WriteWhenReady("G91");
			var formula = $"G2 {moveCode} R4.5 F{speed}";
			WriteWhenReady(formula);
		}
		public void MoveRelativeCounterClockwiseArc(List<MoveSpec> moveSpecs, decimal speed)
		{
			var moveCode = String.Join(' ', moveSpecs.Select(s => s.ToGode()));
			WriteWhenReady("G91");
			var formula = $"G3 {moveCode} R4.5 F{speed}";
			WriteWhenReady(formula);
		}
		public void MovePercent(AxisSpec axisSpec, decimal percent, decimal? speed = null)
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
		public void MoveRelative(AxisSpec axisSpec, decimal distance, decimal? speed = null)
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

			if(Settings.Debug)
			{
				if (sensorDesignation == Settings.DnaSensorNumber)
					return new Random().Next(Settings.DnaSensorReadLow, Settings.DnaSensorReadHigh).ToString();
				if (sensorDesignation == Settings.NadhSensorNumber)
					return new Random().Next(Settings.NadhSensorReadLow, Settings.NadhSensorReadHigh).ToString();
			}

			if (response.StartsWith("Error"))
				return response;
			var first = response.Split("reading ")[1];
			var last = first.Split(",")[0];
			return last;
		}
		/// <summary>
		/// Uploads a local text file to the controller board over the serial connection (M28/M29). The file is written to a temporary
		/// name and only renamed over <paramref name="remotePath"/> once it has been received completely, so a failed upload never
		/// leaves a partial file in its place. Returns null on success, otherwise an error message.
		/// </summary>
		public string? SendFile(string localPath, string remotePath)
		{
			var tempPath = remotePath + ".tmp";
			var started = false;
			try
			{
				var lines = File.ReadAllLines(localPath).Where(l => !String.IsNullOrWhiteSpace(l)).ToList();
				OpenPortIfNeeded();

				while (_fileAck.Wait(0)) { }
				_countFileAcks = true;
				SendFileLine($"M28 \"{tempPath}\"");
				started = true;
				foreach (var line in lines)
					SendFileLine(line);
				SendFileLine("M29");
				started = false;
				SendFileLine($"M471 S\"{tempPath}\" T\"{remotePath}\" D1");
				return null;
			}
			catch (Exception ex)
			{
				Log.Error(ex, $"Error sending {localPath} to {remotePath}");
				if (started)
				{
					try { _serialPort.WriteLine("M29"); } catch { }
				}
				return $"Error: {ex.Message}";
			}
			finally
			{
				_countFileAcks = false;
			}
		}
		/// <summary>
		/// Reboots the controller board (M999). The board does not reply and its USB connection drops, so the serial port is
		/// closed afterwards and the next command reopens it. Returns null on success, otherwise an error message.
		/// </summary>
		public string? Reboot()
		{
			try
			{
				OpenPortIfNeeded();
				_serialPort.WriteLine("M999");
				Thread.Sleep(500);
				_serialPort.Close();
				return null;
			}
			catch (Exception ex)
			{
				Log.Error(ex, "Error rebooting the controller board");
				return $"Error: {ex.Message}";
			}
		}
		/// <summary>
		/// Chooses the serial port to talk to: the port named by <see cref="Settings.ComPortNumber"/>.
		/// If the choice changes, an open port is closed so the next command opens the new one.
		/// </summary>
		public void SelectPort(Settings settings)
		{
			Initialize();
			var name = $"COM{settings.ComPortNumber}";

			_portSelected = true;
			if (_serialPort.PortName == name)
				return;
			if (_serialPort.IsOpen)
				_serialPort.Close();
			_serialPort.PortName = name;
		}
		public void SetTrayTemperature(decimal temperature)
		{
			WriteWhenReady($"M140 S{temperature}");
		}
		/// <summary>Turns the controller's WiFi network interface on (M552 S1) or off (M552 S0).</summary>
		public void SetWifiEnabled(bool enabled)
		{
			WriteWhenReady($"M552 S{(enabled ? 1 : 0)}");
		}
		public void SetWifi(string ssid, string password)
		{
			WriteWhenReady($"M552 S0");
			// a double quote inside a G-code string is written as two double quotes
			WriteWhenReady($"M587 S\"{(ssid ?? "").Replace("\"", "\"\"")}\" P\"{(password ?? "").Replace("\"", "\"\"")}\"");
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
			while(!_disposing)
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
					if (_countFileAcks && message.Trim() == "ok")
						_fileAck.Release();
					else if(message == STR_ready)
						_readyFound = true;
					else if(_saveResponses && message != "ok")
						_responses.Enqueue(message);
					else if(message != "ok")
						ReadData.Enqueue(message);
				}
			}
		}
		private void OpenPortIfNeeded()
		{
			Initialize();
			if (_serialPort.IsOpen)
				return;
			if (!_portSelected)
				SelectPort(Settings);
			_serialPort.Open();
		}
		private void SendFileLine(string line)
		{
			_serialPort.WriteLine(line);
			if (!_fileAck.Wait(5000))
				throw new TimeoutException($"The controller board did not acknowledge: {line}");
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
			OpenPortIfNeeded();

			_serialPort.WriteLine($"{data}");
		}
		private string? WriteWhenReady(string data, string responseKey = null)
		{
			try
			{
				OpenPortIfNeeded();

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
				Log.Error(ex, $"Error writing to serial port: {data}");
				return $"Error: {ex.Message}";
			}
		}
		#endregion

		#region Properties
		/// <summary>The device configuration, created from the current settings the first time it is needed (and again after <see cref="ResetConfiguration"/>).</summary>
		public DeviceConfiguration Configuration
		{
			get
			{
				lock (_configurationLock)
					return _configuration ??= new DeviceConfiguration(_settingsProvider());
			}
		}

		/// <summary>The settings the configuration was built from; the current settings if no configuration has been built yet.</summary>
		public Settings Settings
		{
			get
			{
				lock (_configurationLock)
					return _configuration?.Settings ?? _settingsProvider();
			}
		}
		public ConcurrentQueue<string> ReadData { get; set; } = new();
		#endregion

		#region Events
		public event EventHandler<DataArgs<string>>? DataReceived;
		#endregion

		#region Fields
		readonly Func<Settings> _settingsProvider;
		readonly object _configurationLock = new();
		DeviceConfiguration? _configuration;
		readonly object _initializeLock = new();
		bool _initialized;
		Thread? _publishThread;
		ConcurrentQueue<string> _responses = new();
		SerialPort _serialPort = null!; // created by Initialize()
		bool _saveResponses;
		bool _disposing;
		bool _portSelected;
		readonly SemaphoreSlim _fileAck = new(0);
		volatile bool _countFileAcks;

		private const string STR_sensorKey = "Sensor";
		private const string STR_ready = "ready";
		private bool _readyFound;
		#endregion
	}
}
