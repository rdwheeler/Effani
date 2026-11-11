using Drivetrain.Robotics.Control;

namespace Drivetrain.Robotics.Process
{
	public class SetWifi(string ssid, string password, DeviceControl device) : Command(device)
	{
		protected override void Run()
		{
			Device.SetWifi(_ssid, _password);
		}

		// The password is deliberately left out of the description, which is displayed in the UI.
		public override string Description => $"Set WiFi network to {_ssid}.";
		private readonly string _ssid = ssid;
		private readonly string _password = password;
	}
}
