using Drivetrain.Robotics.Control;
using Drivetrain.Robotics.Process;
using System.Linq;
using Drivetrain.Robotics.Process;

namespace Effani.Device.Control
{
	public class MoveToStartingLocations(EffaniDeviceControl device) : Command(device)
	{
		protected override void Run()
		{
			var device = Device as EffaniDeviceControl;
			foreach (var axis in device.Axes.Values)
			{
				Device.MoveToStarting(axis);
			}
		}

		public override string Description => $"Move all axes to starting locations.";
	}
}
