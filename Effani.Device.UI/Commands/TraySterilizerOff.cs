using Drivetrain.Robotics.Control;

namespace Drivetrain.Robotics.Process
{
	public class TraySterilizerOff(AbalDeviceControl device) : Command(device)
	{
		protected override void Run()
		{
			var device = (AbalDeviceControl)Device;
			device.TraySterilizerOff();
		}

		public override string Description => "Turn off tray sterilizer.";
	}
}
