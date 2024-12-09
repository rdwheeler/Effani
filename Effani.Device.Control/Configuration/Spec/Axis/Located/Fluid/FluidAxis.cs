using System;
using System.Linq;

namespace Effani.Device.Control
{
	public class FluidAxis : LocatedAxis
	{
		public FluidAxis(Axis axis, int board, int port, Coordinate location)
			: base(axis, board, port, location)
        {
            Maximum = 200;
            MaxSpeed = 1000;
            StepsPerMillimeter = 1260f;
			Driver = new DriverSpec(Label)
			{
				Current = 1500,
				Direction = Direction.Backwards,
				Endstop = new EndstopSpec(Label)
                {
                    SensorPort = $"{(board > 0 ? board + "." : null)}io{port}.in",
                    Signal = PinSignal.High,
					Location = EndstopLocation.LowEnd,
					FirstHomingSpeed = 1000
                }
			};
		}
	}
}
