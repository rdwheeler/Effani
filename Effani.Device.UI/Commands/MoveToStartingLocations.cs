using Drivetrain.Robotics.Control;
using System.Linq;

namespace Drivetrain.Robotics.Process
{
	public class MoveToStartingLocations(AbalDeviceControl device) : Command(device)
	{
		protected override void Run()
		{
			var device = Device as AbalDeviceControl;
			foreach (var fluidLine in device.FluidAxes)
			{
				Device.MoveToStarting(fluidLine);
			}
			Device.MoveToStarting(device.Axes[Axis.AspirateFluid]);
			Device.MoveToStarting(device.Axes[Axis.AspirateKoh]);
            Device.MoveToStarting(device.Axes[Axis.AspirateFluidZ]);
            Device.MoveToStarting(device.Axes[Axis.AspirateKohZ]);
		}

		public override string Description => $"Move all axes to starting locations.";
	}
}
