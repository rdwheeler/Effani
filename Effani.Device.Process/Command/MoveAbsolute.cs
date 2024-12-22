using Effani.Device.Control;

namespace Effani.Device.Process
{
	public class MoveAbsolute(float position, DeviceControl device, AxisSpec axisSpec = null) : Command(device)
	{
		protected override void Run()
		{
			if(AxisSpec is null)
				throw new ApplicationException($"{nameof(AxisSpec)} cannot be null in {nameof(MoveAbsolute)}");

			Device.MoveAbsolute(AxisSpec, position);
			Result = $"{AxisSpec.Label} moved to {Position}";
		}

		public override string Description => $"Move {AxisSpec.Label} to {Position}";

		public AxisSpec AxisSpec { get; protected set; } = axisSpec;
		public float Position { get; } = position;
	}
}
