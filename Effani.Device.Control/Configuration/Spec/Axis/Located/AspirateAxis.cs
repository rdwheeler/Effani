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
            MaxSpeed = 600;
            StepsPerMillimeter = 1260f;
            Acceleration = 500;
            Driver = new DriverSpec(Label)
            {
                Current = 1500,
                Direction = Direction.Backwards,
                Endstop = new EndstopSpec(Label)
                {
                    SensorPort = $"{(board > 0 ? board + "." : null)}io{port}.in",
                    Signal = PinSignal.Low,
                    Location = EndstopLocation.LowEnd,
                    FirstHomingSpeed = 400
                }
            };
        }
	}
}
