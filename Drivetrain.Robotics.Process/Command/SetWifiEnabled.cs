using Drivetrain.Robotics.Control;

namespace Drivetrain.Robotics.Process
{
	public class SetWifiEnabled(bool enabled, DeviceControl device) : Command(device)
	{
		protected override void Run()
		{
			Device.SetWifiEnabled(_enabled);
		}

		public override string Description => _enabled ? "Enable WiFi on the controller." : "Disable WiFi on the controller.";
		private readonly bool _enabled = enabled;
	}
}
