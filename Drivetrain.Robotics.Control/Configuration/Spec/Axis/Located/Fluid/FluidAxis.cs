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
            MaxSpeed = 2286; // LMD14S13GF15-120 maximum speed: 38.1 mm/s
            StepsPerMillimeter = 1259.84m; // 1.8 degree step x 16 microsteps / 2.54 mm lead
			Driver = new DriverSpec(Label)
			{
				Current = 1500,
				Direction = Direction.Forwards,
				Endstop = new EndstopSpec(Label)
                {
                    SensorPort = endstopPort.InputDesignation,
                    Signal = PinSignal.High,
					Location = EndstopLocation.HighEnd,
					FirstHomingSpeed = 2286
				}
			};
		}
	}
}
