using System;
using System.Linq;

namespace Drivetrain.Robotics.Control
{
	public class Well : Spec
	{
		public Well(Plate plate, int x, int y, Coordinate location) : base($"Well {plate.Location.X}.{plate.Location.Y}.{x}.{y}")
		{
			Y = y;
			X = x;
			plate.Wells.Add(this);
			Plate = plate;
			Location = location;
			var row = y switch
			{
				0 => "A",
				1 => "B",
				2 => "C",
				3 => "D",
				4 => "E",
				5 => "F",
				6 => "G",
				7 => "H",
				_ => y.ToString()
			};
			var plateRow = plate.Location.Y switch
			{
				0 => "A",
				1 => "B",
				_ => plate.Location.Y.ToString()
			};

			Label = $"{plate.Label}:{row}{x + 1}";
		}
		public override string ToString()
		{
			return Label;
		}
		public Coordinate Location { get; private set; }
		public Plate Plate { get; private set; }
		public int X { get; private set; }
		public int Y { get; private set; }
	}
}
