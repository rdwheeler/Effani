using Drivetrain.Robotics.Control;

namespace Drivetrain.Robotics.Process
{
	public class ChamberUvOn(AbalDeviceControl device) : Command(device)
	{
		protected override void Run()
		{
			var device = (AbalDeviceControl)Device;
			device.AccOn(1);
		}

		public override string Description => "Turn on chamber sterilizer.";
	}
}
