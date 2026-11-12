using Drivetrain.Robotics.Control;
using Drivetrain.Robotics.Process;

namespace Effani.Device.Control
{
	public class ChamberUvOn(EffaniDeviceControl device) : Command(device)
	{
		protected override void Run()
		{
			var device = (EffaniDeviceControl)Device;
			device.AccOn(1);
		}

		public override string Description => "Turn on chamber sterilizer.";
	}
}
