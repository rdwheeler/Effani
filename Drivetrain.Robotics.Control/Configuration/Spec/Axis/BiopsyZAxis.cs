using System;
using System.Linq;

namespace Drivetrain.Robotics.Control
{
	public class BiopsyZAxis : AxisSpec
	{
		public BiopsyZAxis(Axis axis, BoardPort driverPort, BoardPort endstopPort, decimal? startingPosition = null)
			: base(axis, driverPort, endstopPort, startingPosition)
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
                    SensorPort = endstopPort.InputDesignation,
                    Signal = PinSignal.Low,
					Location = EndstopLocation.LowEnd,
                    FirstHomingSpeed = 1000
                }
			};
        }
    }
}
