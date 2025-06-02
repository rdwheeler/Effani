using Drivetrain.Robotics.Control;
using System;
using System.Linq;
using Drivetrain.Robotics.Process;

namespace Effani.Device.Control
{
	public class ReadNadh(AbalDeviceControl device, Action<ReadSensor> resultCallback) : ReadSensor(device, resultCallback)
	{
		protected override void Run()
		{
			var device = (AbalDeviceControl)Device;
			device.LaserNadhOn();
			SensorDesignator = Device.Configuration.NadhSensorNumber;
			PauseTime = Device.Configuration.SensorReadDwell;
			base.Run();
			device.LaserNadhOff();
		}

		public override string Description => "Read NADH";
	}
}
