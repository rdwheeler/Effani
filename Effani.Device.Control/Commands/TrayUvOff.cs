using Drivetrain.Robotics.Control;
using Drivetrain.Robotics.Process;

namespace Effani.Device.Control
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
