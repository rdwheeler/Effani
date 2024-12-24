using System;
using System.Linq;

namespace Drivetrain.Robotics.Control
{
	public class Well : Spec
	{
		public Well(Plate plate, int x, int y, Coordinate location) : base($"Well {plate.Index.X}.{plate.Index.Y}.{x}.{y}")
		{
			Y = y;
			X = x;
			plate.Wells.Add(this);
			Plate = plate;
			Location = location;
			var row = y switch
			{
				7 => "A",
				6 => "B",
				5 => "C",
				4 => "D",
				3 => "E",
				2 => "F",
				1 => "G",
				0 => "H",
				_ => y.ToString()
			};
			var plateRow = plate.Index.Y switch
			{
				3 => "A",
				2 => "B",
				1 => "C",
				0 => "D",
				_ => plate.Index.Y.ToString()
			};

			Label = $"Well {row}{x + 1}, {plate.Label}";
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
