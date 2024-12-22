using Effani.Device.Control;
using System;
using System.Linq;

namespace Effani.Device.Process
{
	public class MoveToStartingLocations(AbalDeviceControl device) : Command(device)
	{
		protected override void Run()
		{
			var device = Device as AbalDeviceControl;
			foreach (var fluidLine in device.FluidAxes)
			{
				Device.MoveAbsolute(fluidLine, Device.Configuration.FluidSyringeHomePosition);
			}
			Device.MoveToStarting(device.Axes[Axis.AspirateFluid]);
			Device.MoveToStarting(device.Axes[Axis.AspirateKoh]);
            Device.MoveToStarting(device.Axes[Axis.AspirateFluidZ]);
            Device.MoveToStarting(device.Axes[Axis.AspirateKohZ]);
			Result = $"All axes moved to starting locations.";
		}

		public override string Description => $"Move all axes to starting locations.";
	}
}
