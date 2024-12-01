using System;
using System.Linq;

namespace Effani.Device.Control
{
	public class LocatedAxis : AxisSpec
	{
		public LocatedAxis(Axis axis, int board, int port, Coordinate location)
			: base(axis, board, port)
		{
			Location = location;
		}

		public Coordinate? Location { get; private set; }
	}
}
