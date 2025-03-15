using Drivetrain.Robotics.Control;

namespace Drivetrain.Robotics.Process
{
	public class TraySterilizerOn(AbalDeviceControl device) : Command(device)
	{
		protected override void Run()
		{
			var device = (AbalDeviceControl)Device;
			device.TraySterilizerOn(0);
		}

		public override string Description => "Turn on tray sterilizer.";
	}
}
