using Drivetrain.Robotics.Control;
using System.Threading;
using Drivetrain.Robotics.Process;

namespace Effani.Device.Control
{
	public class ChamberUvOff(AbalDeviceControl device) : Command(device)
	{
		protected override void Run()
		{
			var device = (AbalDeviceControl)Device;
			device.AccOff(1);
		}

		public override string Description => "Turn off chamber sterilizer.";
	}
}
