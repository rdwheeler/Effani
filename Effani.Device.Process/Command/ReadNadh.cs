using Effani.Device.Control;
using System;
using System.Linq;

namespace Effani.Device.Process
{
	public class ReadNadh(DeviceControl device) : Command(device)
	{
		protected override string Run()
		{
			Device.LaserNadhOn();
			Task.Delay(Device.Configuration.OpticalSensorDwell);
			Result = Device.ReadNadh();
			Device.LaserNadhOff();
			return Result;
		}

		public override string Result { get; protected set; } = "Read NADH";
	}
}
