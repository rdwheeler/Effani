using Effani.Device.Control;
using System;
using System.Linq;

namespace Effani.Device.Process
{
	public class RemoveFluid(int microLiters, AbalDeviceControl device) : Command(device)
	{
		protected override void Run()
        {
			(Device as AbalDeviceControl).RemoveFluid(MicroLiters);
			Result = $"{microLiters}μl of general fluid removed.";
		}

		public override string Description => $"Remove {microLiters}μl of general fluid.";

		public int MicroLiters { get; } = microLiters;
	}
}
