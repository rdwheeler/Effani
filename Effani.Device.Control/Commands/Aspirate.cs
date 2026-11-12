using Drivetrain.Robotics.Control;
using System;
using System.Linq;
using Drivetrain.Robotics.Process;

namespace Effani.Device.Control
{
	public class Aspirate(int microLiters, EffaniDeviceControl device) : Command(device)
	{
		protected override void Run()
		{
			(Device as EffaniDeviceControl).Aspirate(MicroLiters);
		}

		public override string Description => $"Aspirate {MicroLiters}μl of KOH.";

		public int MicroLiters { get; } = microLiters;
	}
}
