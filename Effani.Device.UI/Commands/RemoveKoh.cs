using Effani.Device.Control;
using System;
using System.Linq;

namespace Effani.Device.Process
{
	public class RemoveKoh(int microLiters, AbalDeviceControl device) : Command(device)
	{
		protected override void Run()
		{
			(Device as AbalDeviceControl).RemoveKoh(MicroLiters);
			Result = $"{microLiters}μl of KOH removed.";
		}

		public override string Description => $"Remove {microLiters}μl of KOH.";

		public int MicroLiters { get; } = microLiters;
	}
}
