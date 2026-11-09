using System;
using System.Linq;

namespace Drivetrain.Robotics.Control
{
	public class LocatedAxis : AxisSpec
	{
		public LocatedAxis(Axis axis, BoardPort driverPort, BoardPort endstopPort, Coordinate location, decimal? startingPosition = null)
			: base(axis, driverPort, endstopPort, startingPosition)
		{
			Location = location;
		}

		public Coordinate? Location { get; private set; }
	}
}
