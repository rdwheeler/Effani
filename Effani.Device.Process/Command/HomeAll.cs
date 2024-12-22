using Effani.Device.Control;

namespace Effani.Device.Process
{
	public class HomeAll(DeviceControl device) : Command(device)
	{
		protected override void Run()
		{
			Device.HomeAll();
			Result = $"All axes homed.";
		}

		public override string Description => $"Home all axes.";
	}
}
