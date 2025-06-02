using Drivetrain.Robotics.Control;
using System;
using System.Linq;
using Drivetrain.Robotics.Process;

namespace Effani.Device.Control
{
	public class ReadDna(AbalDeviceControl device, Action<ReadSensor> resultCallback) : ReadSensor(device, resultCallback)
	{
		protected override void Run()
		{
			var device = (AbalDeviceControl)Device;
			device.LaserDnaOn();
			SensorDesignator = Device.Configuration.DnaSensorNumber;
			PauseTime = Device.Configuration.SensorReadDwell;
			base.Run();
			device.LaserDnaOff();
		}

		public override string Description => "Read DNA";
	}
}
