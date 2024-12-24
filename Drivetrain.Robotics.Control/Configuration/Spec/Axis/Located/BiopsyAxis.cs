using System;
using System.Linq;

namespace Drivetrain.Robotics.Control
{
	public class BiopsyAxis : LocatedAxis
	{
		public BiopsyAxis(Axis axis, int board, int port, Coordinate location, float? startingPosition = null)
			: base(axis, board, port, location, startingPosition)
        {
            Maximum = 200;
            MaxSpeed = 1000;
            StepsPerMillimeter = 3200f;
            Driver = new DriverSpec(Label)
			{
				Current = 1500,
				Direction = Direction.Forwards,
				Endstop = new EndstopSpec(Label)
                {
                    SensorPort = $"{(board > 0 ? board + "." : null)}io{port}.in",
                    Signal = PinSignal.Low,
					Location = EndstopLocation.LowEnd,
					FirstHomingSpeed = 1000
				}
			};
		}
	}
}
