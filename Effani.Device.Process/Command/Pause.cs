using Effani.Device.Control;
using System;
using System.Linq;

namespace Effani.Device.Process
{
    public class Pause(int milliseconds, DeviceControl device) : Command(device)
    {
        protected override void Run()
        {
            Device.Pause(Milliseconds);
            Result = $"Pauseed for {milliseconds} milliseconds.";
        }

        public override string Description => $"Pause for {milliseconds} milliseconds.";

        public int Milliseconds { get; } = milliseconds;
    }
}
