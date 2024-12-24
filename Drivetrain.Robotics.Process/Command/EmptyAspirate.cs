using Drivetrain.Robotics.Control;

namespace Drivetrain.Robotics.Process
{
	public class EmptyAspirate(DeviceControl device, AspirateAxis axisSpec = null) : Command(device)
	{
		protected override void Run()
		{
			if (AxisSpec is null)
				throw new ApplicationException($"{nameof(AxisSpec)} cannot be null in {nameof(EmptyAspirate)}");

			Device.MoveAbsolute(AxisSpec, AxisSpec.StartingPosition);
		}

		public override string Description => $"Empty {AxisSpec?.Label}.";

		protected AspirateAxis AxisSpec = axisSpec;
	}
}
