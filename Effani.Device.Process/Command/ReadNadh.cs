using Effani.Device.Control;
using System;
using System.Linq;

namespace Effani.Device.Process
{
	public class ReadNadh(DeviceControl device) : ReadCommand(device)
	{
		protected override string Run()
		{
			Device.LaserNadhOn();
			Result = Device.ReadNadh();
			Device.LaserNadhOff();
			return Result;
		}

		public override string Result { get; protected set; } = "Read NADH";
	}
}
