using Effani.Device.Control;
using System;
using System.Linq;

namespace Effani.Device.Process
{
	public class ReadDna(AbalDeviceControl device) : ReadSensor(device)
	{
		protected override void Run()
		{
			var device = (AbalDeviceControl)Device;
			device.LaserDnaOn();
			SensorDesignator = 0;
			PauseTime = Device.Configuration.SensorReadDwell;
			base.Run();
			device.LaserDnaOff();
		}

		public override string Description => "Read DNA.";
	}
}
