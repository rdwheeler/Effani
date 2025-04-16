using Drivetrain.Robotics.Control;
using System;
using System.Linq;

namespace Drivetrain.Robotics.Process
{
	public class AspirateKoh(int microLiters, AbalDeviceControl device) : Command(device)
	{
		protected override void Run()
		{
			(Device as AbalDeviceControl).AspirateKoh(MicroLiters);
		}

		public override string Description => $"Aspirate {MicroLiters}μl of KOH.";

		public int MicroLiters { get; } = microLiters;
	}
}
