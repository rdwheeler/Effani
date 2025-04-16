using System;
using System.Linq;

namespace Drivetrain.Robotics.Control
{
	public class Well : Spec
	{
		public Well(Plate plate, int x, int y, Coordinate location) : base(null)
		{
			Y = y;
			X = x;
			plate.Wells.Add(this);
			Plate = plate;
			Location = location;
			var row = x switch
			{
				0 => "A",
				1 => "B",
				2 => "C",
				3 => "D",
				4 => "E",
				5 => "F",
				6 => "G",
				7 => "H",
				8 => "I",
				9 => "J",
				10 => "K",
				11 => "L",
				12 => "M",
				13 => "N",
				_ => x.ToString()
			};

			Label = $"{plate.Label}:{row}{y + 1}";
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
