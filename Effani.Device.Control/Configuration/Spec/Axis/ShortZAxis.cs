using System;
using System.Linq;

namespace Effani.Device.Control
{
	public class ShortZAxis : AxisSpec
	{
		public ShortZAxis(Axis axis, int board, int port)
			: base(axis, board, port)
		{
			Maximum = 35;
			StepsPerMillimeter = 6400f;
			MaxSpeed = 3;
            Acceleration = 3;
			Driver = new DriverSpec(Label)
			{
                Current = 600,
				Direction = Direction.Forwards,
				Endstop = new EndstopSpec(Label)
                {
                    SensorPort = $"{(board > 0 ? board + "." : null)}io{port}.in",
                    Signal = PinSignal.Low,
					Location = EndstopLocation.LowEnd,
					FirstHomingSpeed = 3,
                    SecondHomingSpeed = 3
				}
			};
		}
	}
}
