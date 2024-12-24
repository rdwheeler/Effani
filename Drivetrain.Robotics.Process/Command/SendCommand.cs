using Drivetrain.Robotics.Control;

namespace Drivetrain.Robotics.Process
{
	public class SendCommand(string command, DeviceControl device) : Command(device)
	{
		protected override void Run()
		{
			Device.SendCommand(_command);
		}

		public override string Description => $"Send {_command}.";
		private readonly string _command = command;
	}
}
