using System;
using System.Linq;

namespace Drivetrain.Robotics.Control
{
	public class LocatedAxis : AxisSpec
	{
		public LocatedAxis(Axis axis, int board, int port, Coordinate location, float? startingPosition = null)
			: base(axis, board, port, startingPosition)
		{
			Location = location;
		}

		public Coordinate? Location { get; private set; }
	}
}
