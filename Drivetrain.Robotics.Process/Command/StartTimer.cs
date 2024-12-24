using Drivetrain.Robotics.Control;
using System.Linq;
using System.Timers;

namespace Drivetrain.Robotics.Process
{
	public class StartTimer(System.Timers.Timer timer, string name, DeviceControl device) : Command(device)
	{

        protected override void Run()
		{
            _timer.Start();
		}

		public override string Description => $"Start timer {_name}.";

        private readonly System.Timers.Timer _timer = timer;
        private readonly string _name = name;
	}
}
