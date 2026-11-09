using System;
using System.Linq;

namespace Drivetrain.Robotics.Control
{
	public class TableAxis : AxisSpec
	{
		public TableAxis(Axis axis, BoardPort driverPort, BoardPort endstopPort, int maximumMovement, EndstopLocation endstopLocation, Direction direction, int maxSpeed, decimal? startingPosition = null)
			: base(axis, driverPort, endstopPort, startingPosition)
		{
			Maximum = maximumMovement;
			StepsPerMillimeter = 400;
			MaxSpeed = maxSpeed;
			Driver = new DriverSpec(Label)
			{
				Current = 2000,
				Direction = direction,
				Endstop = new EndstopSpec($"{Label} Endstop")
                {
                    SensorPort = endstopPort.InputDesignation,
                    Signal = PinSignal.High,
					Location = endstopLocation,
					FirstHomingSpeed = 3000
				}
			};
		}
	}
}
