using Effani.Device.Control;

namespace Effani.Device.Process
{
	public class Home(DeviceControl device, AxisSpec axisSpec = null) : Command(device)
	{
		protected override void Run()
		{
			Device.Home(AxisSpec);
			Result = $"{AxisSpec.Label} homed.";
		}

		public override string Description => $"{AxisSpec.Label} homed.";
		public AxisSpec AxisSpec { get; set; } = axisSpec;
	}
}
