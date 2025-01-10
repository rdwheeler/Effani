using Drivetrain.Robotics.Control;
using System;
using System.Linq;

namespace Drivetrain.Robotics.Process
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
