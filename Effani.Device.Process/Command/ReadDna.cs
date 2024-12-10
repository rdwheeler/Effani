using Effani.Device.Control;
using System;
using System.Linq;

namespace Effani.Device.Process
{
	public class ReadDna(DeviceControl device) : ReadCommand(device)
	{
		protected override string Run()
		{
			Device.LaserDnaOn();
			Result = Device.ReadDna();
			Device.LaserDnaOff();
			return Result;
		}

		public override string Result { get; protected set; } = "DNA read";
	}
}
