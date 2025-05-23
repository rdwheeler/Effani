using Drivetrain.Robotics.Control;
using System.Threading;

namespace Drivetrain.Robotics.Process
{
	public class RunSterilizationCycle(AbalDeviceControl device, int dwell) : Command(device)
	{

		protected override void Run()
		{
			var device = (AbalDeviceControl)Device;
			device.AccOn(0);
			Thread.Sleep(dwell);
			device.AccOff(0);
			device.AccOn(1);
			Thread.Sleep(dwell);
			device.AccOff(1);
		}
		public override string Description => $"Sterilizing for {dwell/1000 * 2} seconds.";

		private readonly int dwell = dwell;
	}
}
