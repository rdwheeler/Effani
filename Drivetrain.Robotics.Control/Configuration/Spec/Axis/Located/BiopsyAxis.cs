using System;
using System.Linq;

namespace Drivetrain.Robotics.Control
{
	public class BiopsyAxis : LocatedAxis
	{
		public BiopsyAxis(Axis axis, BoardPort driverPort, BoardPort endstopPort, Coordinate location, decimal? startingPosition = null)
			: base(axis, driverPort, endstopPort, location, startingPosition)
        {
            Maximum = 200;
            MaxSpeed = 1000;
            StepsPerMillimeter = 3200m;
            Driver = new DriverSpec(Label)
			{
				Current = 1500,
				Direction = Direction.Forwards,
				Endstop = new EndstopSpec(Label)
                {
                    SensorPort = endstopPort.InputDesignation,
                    Signal = PinSignal.Low,
					Location = EndstopLocation.LowEnd,
					FirstHomingSpeed = 1000
				}
			};
		}
	}
}
