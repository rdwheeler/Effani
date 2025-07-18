using Drivetrain.Robotics.Control;

namespace Drivetrain.Robotics.Process
{
	public class MoveAbsolute(decimal position, DeviceControl device, AxisSpec axisSpec = null) : Command(device)
	{
		protected override void Run()
		{
			if(AxisSpec is null)
				throw new ApplicationException($"{nameof(AxisSpec)} cannot be null in {nameof(MoveAbsolute)}");

			Device.MoveAbsolute(AxisSpec, Position);
		}

		public override string Description => $"Move {AxisSpec.Label} to {Position}";

		public AxisSpec AxisSpec { get; protected set; } = axisSpec;
		public decimal Position { get; } = position;
	}
}
