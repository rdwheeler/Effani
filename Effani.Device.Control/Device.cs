using System.IO.Ports;

namespace Effani.Device.Control
{
	public class Device
	{
		public void Move()
		{
			Thread readThread = new Thread(Read);
			_continue = true;
			_serialPort = new SerialPort("COM5", 9600, Parity.None, 8, StopBits.One);
			_serialPort.Handshake = Handshake.None;
			_serialPort.ReadTimeout = 500;
			_serialPort.WriteTimeout = 500;
			_serialPort.DataReceived += _serialPort_DataReceived;
			try
			{
				_serialPort.Open();
				_serialPort.Write("G0 X12\r\n");
			}
			catch (Exception ex)
			{
				Console.WriteLine("Error opening/writing to serial port :: " + ex.Message, "Error!");
			}
		}

		private void _serialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
		{
			Thread.Sleep(500);
			string data = _serialPort.ReadLine();
			Console.WriteLine(e.EventType.ToString());
			Console.WriteLine(e.ToString());
			Console.WriteLine(data);
		}
		public void Read()
		{
			while (_continue)
			{
				try
				{
					string message = _serialPort.ReadLine();
					Console.WriteLine(message);
				}
				catch (TimeoutException) { }
			}
		}

		SerialPort _serialPort;
		static bool _continue;
	}
}
