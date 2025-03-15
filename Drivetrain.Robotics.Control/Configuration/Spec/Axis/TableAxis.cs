using System;
using System.Linq;

namespace Drivetrain.Robotics.Control
{
	public class TableAxis : AxisSpec
	{
		public TableAxis(Axis axis, int board, int port, int maximumMovement, EndstopLocation endstopLocation, Direction direction, int maxSpeed, float? startingPosition = null)
			: base(axis, board, port, startingPosition)
		{
			Maximum = maximumMovement;
			StepsPerMillimeter = 400;
			MaxSpeed = maxSpeed;
			Driver = new DriverSpec(Label)
			{
				Current = 1700,
				Direction = direction,
				Endstop = new EndstopSpec($"{Label} Endstop")
                {
                    SensorPort = $"{(board > 0 ? board + "." : null)}io{port}.in",
                    Signal = PinSignal.High,
					Location = endstopLocation,
					FirstHomingSpeed = 3000
				}
			};
		}
	}
}
