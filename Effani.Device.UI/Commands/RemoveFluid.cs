using Drivetrain.Robotics.Control;
using System;
using System.Linq;

namespace Drivetrain.Robotics.Process
{
	public class RemoveFluid(int microLiters, AbalDeviceControl device) : Command(device)
	{
		protected override void Run()
        {
			(Device as AbalDeviceControl).RemoveFluid(MicroLiters);
		}

		public override string Description => $"Remove {MicroLiters}μl of general fluid.";

		public int MicroLiters { get; } = microLiters;
	}
}
