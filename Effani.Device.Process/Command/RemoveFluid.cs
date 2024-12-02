using Effani.Device.Control;
using System;
using System.Linq;

namespace Effani.Device.Process
{
	public class RemoveFluid(int microLiters, DeviceControl device) : Command(device)
	{
		protected override string Run()
		{
			Result = Device.RemoveFluid(MicroLiters);
			return Result;
		}

		public override string Result { get; protected set; } = $"Fluid removed";

		public int MicroLiters { get; } = microLiters;
	}
}
