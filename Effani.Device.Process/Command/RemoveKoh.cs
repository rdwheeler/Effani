using Effani.Device.Control;
using System;
using System.Linq;

namespace Effani.Device.Process
{
	public class RemoveKoh(int microLiters, DeviceControl device) : Command(device)
	{
		protected override string Run()
		{
			Result = Device.RemoveKoh(MicroLiters);
			return Result;
		}

		public override string Result { get; protected set; } = $"KOH removed";
		public int MicroLiters { get; } = microLiters;
	}
}
