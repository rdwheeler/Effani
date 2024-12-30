using Drivetrain.Robotics.Control;
using System;
using System.Linq;

namespace Drivetrain.Robotics.Process
{
	public class AlignWellToLocation : MoveTableAbsolute
	{
		public AlignWellToLocation(Coordinate location, Well well, AbalDeviceControl device) : base(device)
		{
			_location = location;
			_well = well;

			var x = location.X - well.Location.X;
			var y = location.Y - well.Location.Y;
			Coordinate = new Coordinate(x, y);
		}

		public override string Description => $"Align {_well.Label} with {_location.X},{_location.Y}";

		private readonly Coordinate _location;
		private readonly Well _well;

	}
}
