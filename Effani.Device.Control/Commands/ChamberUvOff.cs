using Drivetrain.Robotics.Control;
using System.Threading;
using Drivetrain.Robotics.Process;

namespace Effani.Device.Control
{
	public class ChamberUvOff(EffaniDeviceControl device) : Command(device)
	{
		protected override void Run()
		{
			var device = (EffaniDeviceControl)Device;
			device.AccOff(1);
		}

		public override string Description => "Turn off chamber sterilizer.";
	}
}
