using Drivetrain.Robotics.Control;
using System.Threading;

namespace Drivetrain.Robotics.Process
{
	public class TraySterilizerOff(AbalDeviceControl device) : Command(device)
	{
		protected override void Run()
		{
			var device = (AbalDeviceControl)Device;
			device.TraySterilizerOff(0);
		}

		public override string Description => "Turn off tray sterilizer.";
	}
}
