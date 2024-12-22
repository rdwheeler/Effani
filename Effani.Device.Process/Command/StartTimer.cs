using Effani.Device.Control;
using System.Linq;
using System.Timers;

namespace Effani.Device.Process
{
	public class StartTimer(System.Timers.Timer timer, string name, DeviceControl device) : Command(device)
	{

        protected override void Run()
		{
            timer.Start();
            Result = $"Timer {name} started.";
		}

		public override string Description => $"Start timer {name}.";

        private readonly System.Timers.Timer timer = timer;
        private readonly string name = name;
	}
}
