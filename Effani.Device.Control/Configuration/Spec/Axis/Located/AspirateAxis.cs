using System;
using System.Linq;

namespace Effani.Device.Control
{
	public class AspirateAxis : LocatedAxis
	{
		public AspirateAxis(Axis axis, int board, int port, Coordinate location)
			: base(axis, board, port, location)
		{
			Maximum = 150;
			StepsPerMillimeter = 2133.33f;
			MaxSpeed = 2280;
			Driver = new DriverSpec(Label)
			{
				Direction.Forwards,
				Endstop = new EndstopSpec(Label)
				{
					SensorPort = $"{(board > 0 ? board : null)}.io{port}.in",
					Signal = PinSignal.Low,
					Location = EndstopLocation.LowEnd,
					FirstHomingSpeed = 1000
				}
			};
		}
	}
}
