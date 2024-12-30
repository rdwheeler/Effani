using Drivetrain.Robotics.Control;
using System.Linq;

namespace Drivetrain.Robotics.Process
{
	public class MoveToStartingLocations(AbalDeviceControl device) : Command(device)
	{
		protected override void Run()
		{
			var device = Device as AbalDeviceControl;
			foreach (var axis in device.Axes.Values)
			{
				Device.MoveToStarting(axis);
			}
		}

		public override string Description => $"Move all axes to starting locations.";
	}
}
