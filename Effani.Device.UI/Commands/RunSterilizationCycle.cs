using Drivetrain.Robotics.Control;
using System.Threading;

namespace Drivetrain.Robotics.Process
{
	public class RunSterilizationCycle(AbalDeviceControl device, int dwell) : Command(device)
	{

		protected override void Run()
		{
			var device = (AbalDeviceControl)Device;
			device.TraySterilizerOn(0);
			Thread.Sleep(dwell);
			device.TraySterilizerOff(0);
			device.TraySterilizerOn(1);
			Thread.Sleep(dwell);
			device.TraySterilizerOff(1);
		}
		public override string Description => $"Sterilizing for {dwell/1000} seconds.";

		private readonly int dwell = dwell;
	}
}
