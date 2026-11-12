using Drivetrain.Robotics.Control;
using System;
using System.Linq;
using Drivetrain.Robotics.Process;

namespace Effani.Device.Control
{
	public class ReadNadh(EffaniDeviceControl device, Action<ReadSensor> resultCallback) : ReadSensor(device, resultCallback)
	{
		protected override void Run()
		{
			var device = (EffaniDeviceControl)Device;
			device.LaserNadhOn();
			SensorDesignator = Device.Configuration.Settings.NadhSensorNumber;
			PauseTime = Device.Configuration.Settings.SensorReadDwell;
			base.Run();
			device.LaserNadhOff();
		}

		public override string Description => "Read NADH";
	}
}
