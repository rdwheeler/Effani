using Effani.Device.Control;
using System;
using System.Linq;

namespace Effani.Device.Process
{
	public class ReadNadh(AbalDeviceControl device) : ReadSensor(device)
	{
		protected override void Run()
		{
			var device = (AbalDeviceControl)Device;
			device.LaserNadhOn();
			SensorDesignator = 1;
			PauseTime = Device.Configuration.SensorReadDwell;
			base.Run();
			device.LaserNadhOff();
		}

		public override string Description => "Read NADH.";
	}
}
