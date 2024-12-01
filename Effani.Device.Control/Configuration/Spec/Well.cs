using System;
using System.Linq;

namespace Effani.Device.Control
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
		}
		public Coordinate Location { get; private set; }
		public Plate Plate { get; private set; }
		public int X { get; private set; }
		public int Y { get; private set; }
	}
}
