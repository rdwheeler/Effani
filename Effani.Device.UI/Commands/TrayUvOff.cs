using Drivetrain.Robotics.Control;

namespace Drivetrain.Robotics.Process
{
	public class TrayUvOff(AbalDeviceControl device) : Command(device)
	{
		protected override void Run()
		{
			var device = (AbalDeviceControl)Device;
			device.AccOff(0);
		}

		public override string Description => "Turn off chamber sterilizer.";
	}
}
