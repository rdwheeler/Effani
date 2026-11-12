using Drivetrain.Robotics.Control;
using Drivetrain.Robotics.Process;

namespace Effani.Device.Control
{
	public class TrayUvOff(EffaniDeviceControl device) : Command(device)
	{
		protected override void Run()
		{
			var device = (EffaniDeviceControl)Device;
			device.AccOff(0);
		}

		public override string Description => "Turn off chamber sterilizer.";
	}
}
