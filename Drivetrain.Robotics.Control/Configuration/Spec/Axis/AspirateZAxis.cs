using System;
using System.Linq;

namespace Drivetrain.Robotics.Control
{
    public class AspirateZAxis : AxisSpec
    {
        public AspirateZAxis(Axis axis, int board, int port, float? startingPosition = null)
            : base(axis, board, port, startingPosition)
        {
            Maximum = 60;
            MaxSpeed = 600;
            StepsPerMillimeter = 3200f;
            Driver = new DriverSpec(Label)
            {
                Current = 1500,
                Direction = Direction.Backwards,
                Endstop = new EndstopSpec(Label)
                {
                    SensorPort = $"{(board > 0 ? board + "." : null)}io{port}.in",
                    Signal = PinSignal.Low,
                    Location = EndstopLocation.LowEnd,
                    FirstHomingSpeed = 600
                }
            };
        }
    }
}
