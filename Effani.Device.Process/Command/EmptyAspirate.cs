using Effani.Device.Control;

namespace Effani.Device.Process
{
	public class EmptyAspirate(DeviceControl device, AspirateAxis axisSpec = null) : Command(device)
	{
		protected override void Run()
		{
			if (AxisSpec is null)
				throw new ApplicationException($"{nameof(AxisSpec)} cannot be null in {nameof(EmptyAspirate)}");

			Device.MoveAbsolute(axisSpec, axisSpec.StartingPosition);

			Result = $"{AxisSpec?.Label} emptied.";
		}

		public override string Description => $"Empty {AxisSpec?.Label}.";

		protected AspirateAxis AxisSpec = axisSpec;
	}
}
