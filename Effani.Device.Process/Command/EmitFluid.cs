using Effani.Device.Control;
using System;
using System.Linq;

namespace Effani.Device.Process
{
	public class EmitFluid(Axis axis, int microLiters, DeviceControl device) : Command(device)
	{
		protected override string Run()
		{
			Result = Device.EmitFluid(Axis, MicroLiters);
			return Result;
		}

		public override string Result { get; protected set; } = $"Fluid emitted";

		public Axis Axis { get; } = axis;
		public int MicroLiters { get; } = microLiters;
    }
}
