using Drivetrain.Robotics.Control;

namespace Drivetrain.Robotics.Process
{
	public class TrayUvOn(AbalDeviceControl device) : Command(device)
	{
		protected override void Run()
		{
			var device = (AbalDeviceControl)Device;
			device.AccOn(0);
		}

		public override string Description => "Turn on chamber sterilizer.";
	}
}
