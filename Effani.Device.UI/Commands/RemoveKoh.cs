using Drivetrain.Robotics.Control;
using System;
using System.Linq;

namespace Drivetrain.Robotics.Process
{
	public class RemoveKoh(int microLiters, AbalDeviceControl device) : Command(device)
	{
		protected override void Run()
		{
			(Device as AbalDeviceControl).RemoveKoh(MicroLiters);
		}

		public override string Description => $"Remove {MicroLiters}μl of KOH.";

		public int MicroLiters { get; } = microLiters;
	}
}
