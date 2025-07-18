using System;
using System.Linq;

namespace Drivetrain.Robotics.Control
{
	public class BiopsyZAxis : AxisSpec
	{
		public BiopsyZAxis(Axis axis, int board, int port, decimal? startingPosition = null)
			: base(axis, board, port, startingPosition)
		{
			Maximum = 100;
			StepsPerMillimeter = 3200m;
			MaxSpeed = 400;
			Driver = new DriverSpec(Label)
			{
                Current = 1500,
				Direction = Direction.Backwards,
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
