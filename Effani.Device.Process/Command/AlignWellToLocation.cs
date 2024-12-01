using Effani.Device.Control;
using System;
using System.Linq;

namespace Effani.Device.Process
{
	public class AlignWellToLocation(Coordinate location, Well well, DeviceControl device) : Command(device)
	{

		public override void Execute()
		{
			var x = location.X - _well.Location.X;
			var y = location.Y - _well.Location.Y;
			Device.Move(Axis.TableX, x);
			Device.Move(Axis.TableY, y);
			Result = $" {_well.Label} aligned with {_location.X}.{_location.Y}";
		}

		private readonly Coordinate _location = location;
		private readonly Well _well = well;
	}
}
