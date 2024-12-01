using Effani.Device.Control;
using System;
using System.Linq;

namespace Effani.Device.Process
{
	public class EmitFluid(Axis axis, int microLiters, DeviceControl device) : Command(device)
	{
		public override void Execute()
		{
			Result = Device.EmitFluid(Axis, MicroLiters);
		}
		public Axis Axis { get; } = axis;
		public int MicroLiters { get; } = microLiters;
	}
}
