using Drivetrain.Robotics.Control;

namespace Drivetrain.Robotics.Process
{
	public class HomeAll(DeviceControl device) : Command(device)
	{
		protected override void Run()
		{
			Device.HomeAll();
		}

		public override string Description => $"Home all axes.";
	}
}
