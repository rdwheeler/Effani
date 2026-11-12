using Drivetrain.Robotics.Control;
using System;
using System.Linq;
using Drivetrain.Robotics.Process;

namespace Effani.Device.Control
{
	public class ReadDna(EffaniDeviceControl device, Action<ReadSensor> resultCallback) : ReadSensor(device, resultCallback)
	{
		protected override void Run()
		{
			var device = (EffaniDeviceControl)Device;
			device.LaserDnaOn();
			SensorDesignator = Device.Configuration.Settings.DnaSensorNumber;
			PauseTime = Device.Configuration.Settings.SensorReadDwell;
			base.Run();
			device.LaserDnaOff();
		}

		public override string Description => "Read DNA";
	}
}
