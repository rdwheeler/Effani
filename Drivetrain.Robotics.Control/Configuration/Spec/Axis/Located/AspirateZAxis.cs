using System;
using System.Linq;

namespace Drivetrain.Robotics.Control
{
    public class AspirateZAxis : LocatedAxis
    {
        public AspirateZAxis(Axis axis, int board, int port, Coordinate location, decimal? startingPosition = null)
            : base(axis, board, port, location, startingPosition)
        {
            Maximum = 500;
            MaxSpeed = 4000;
            StepsPerMillimeter = 1259.84m;
            Driver = new DriverSpec(Label)
            {
                Current = 500,
                Direction = Direction.Forwards,
                Endstop = new EndstopSpec(Label)
                {
                    SensorPort = $"{(board > 0 ? board + "." : null)}io{port}.in",
                    Signal = PinSignal.High,
                    Location = EndstopLocation.HighEnd,
                    FirstHomingSpeed = 4000
				}
            };
        }
    }
}
