using Effani.Device.Control;
using System.Linq;
using System.Timers;

namespace Effani.Device.Process
{
	public class StartTimer(System.Timers.Timer timer, string name, DeviceControl device) : Command(device)
	{

        protected override string Run()
		{
            timer.Start();
            return $"Timer {name} started";
		}

		public override string Result { get; protected set; } = $"Timer started";

        private readonly System.Timers.Timer timer = timer;
        private readonly string name = name;
	}
}
