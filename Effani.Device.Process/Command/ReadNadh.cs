using Effani.Device.Control;
using System;
using System.Linq;

namespace Effani.Device.Process
{
	public class ReadNadh(DeviceControl device) : Command(device)
	{
		public override void Run()
		{
			Device.LaserNadhOn();
			Task.Delay(Device.Configuration.OpticalSensorDwell);
			Result = Device.ReadNadh();
			Device.LaserNadhOff();
		}
	}
}
