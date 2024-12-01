using Effani.Device.Control;
using System;
using System.Linq;

namespace Effani.Device.Process
{
	public class EmitCo2(DeviceControl device) : Command(device)
	{
		public override void Run()
		{
			Result = Device.EmitCo2(Device.Configuration.Co2Dwell);
		}
	}
}
