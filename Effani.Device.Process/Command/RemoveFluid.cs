using Effani.Device.Control;
using System;
using System.Linq;

namespace Effani.Device.Process
{
	public class RemoveFluid(Axis axis, int microLiters, DeviceControl device) : Command(device)
	{
		public override void Run()
		{
			Result = Device.RemoveFluid(Axis, MicroLiters);
		}
		public Axis Axis { get; } = axis;
		public int MicroLiters { get; } = microLiters;
	}
}
