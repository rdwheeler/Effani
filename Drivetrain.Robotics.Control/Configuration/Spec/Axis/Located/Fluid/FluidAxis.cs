using System;
using System.Linq;

namespace Drivetrain.Robotics.Control
{
	public class FluidAxis : LocatedAxis
	{
		public FluidAxis(Axis axis, BoardPort driverPort, BoardPort endstopPort, Coordinate location, decimal? startingPosition = null)
			: base(axis, driverPort, endstopPort, location, startingPosition)
        {
            Maximum = 117;
            MaxSpeed = 4000;
            StepsPerMillimeter = 1260m;
			Driver = new DriverSpec(Label)
			{
				Current = 1500,
				Direction = Direction.Forwards,
				Endstop = new EndstopSpec(Label)
                {
                    SensorPort = endstopPort.InputDesignation,
                    Signal = PinSignal.High,
					Location = EndstopLocation.HighEnd,
					FirstHomingSpeed = 4000
				}
			};
		}
	}
}
