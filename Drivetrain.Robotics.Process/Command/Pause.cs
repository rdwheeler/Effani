using Drivetrain.Robotics.Control;
using System;
using System.Linq;

namespace Drivetrain.Robotics.Process
{
    public class Pause(int milliseconds, DeviceControl device) : Command(device)
    {
        protected override void Run()
        {
            Device.Pause(Milliseconds);
        }

        public override string Description => $"Pause for {Milliseconds} milliseconds.";

        public int Milliseconds { get; } = milliseconds;
    }
}
