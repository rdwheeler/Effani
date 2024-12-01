using Effani.Device.Control;
using System;
using System.Linq;

namespace Effani.Device.Process
{
	public class ReadDna(DeviceControl device) : Command(device)
	{
		public override void Execute()
		{
			Device.LaserDnaOn();
			Task.Delay(Device.Configuration.OpticalSensorDwell);
			Result = Device.ReadDna();
			Device.LaserDnaOff();
		}
	}
}
