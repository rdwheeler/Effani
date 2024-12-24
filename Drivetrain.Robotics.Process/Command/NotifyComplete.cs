using Drivetrain.Robotics.Control;

namespace Drivetrain.Robotics.Process
{
	public class NotifyComplete(Action<Command> completeCallback, string description, DeviceControl device) : Command(device)
	{
		protected override void Run()
		{
			_completeCallback?.Invoke(this);
		}

		public override string Description => _description;

		private Action<Command> _completeCallback = completeCallback;
		private readonly string _description = description;
	}
}
