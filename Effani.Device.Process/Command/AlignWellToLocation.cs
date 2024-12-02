using Effani.Device.Control;
using System;
using System.Linq;

namespace Effani.Device.Process
{
	public class AlignWellToLocation(Coordinate location, Well well, DeviceControl device) : Command(device)
	{

		protected override string Run()
		{
			var x = location.X - _well.Location.X;
			var y = location.Y - _well.Location.Y;
			Device.TableGoTo(x, y);
			Result =  $"{_well.Label} aligned with {_location.X}.{_location.Y}";
			return Result;
		}

		public override string Result { get; protected set; } = $"Well aligned with location";

		private readonly Coordinate _location = location;
		private readonly Well _well = well;
	}
}
