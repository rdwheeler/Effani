using System;
using System.Linq;

namespace Effani.Device.Control
{
	public class AspirateAxis : LocatedAxis
	{
		public AspirateAxis(Axis axis, int board, int port, Coordinate location)
			: base(axis, board, port, location)
        {
            Maximum = 200;
            MaxSpeed = 400;
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
