using Drivetrain.Robotics.Control;

namespace Drivetrain.Robotics.Process
{
	public class Home(DeviceControl device, AxisSpec axisSpec = null) : Command(device)
	{
		protected override void Run()
		{
			Device.Home(AxisSpec);
		}

		public override string Description => $"Home {AxisSpec.Label}.";
		public AxisSpec AxisSpec { get; set; } = axisSpec;
	}
}
