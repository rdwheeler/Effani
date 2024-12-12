using Effani.Device.Control;
using System;
using System.Linq;

namespace Effani.Device.Process
{
    public class Pause(int milliseconds, DeviceControl device) : Command(device)
    {
        protected override string Run()
        {
            Result = Device.Pause(Milliseconds);
            return Result;
        }

        public override string Result { get; protected set; } = $"Pause";

        public int Milliseconds { get; } = milliseconds;
    }
}
