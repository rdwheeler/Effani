using System;
using System.Linq;

namespace Effani.Device.Control
{
	public class ShortZAxis : AxisSpec
	{
		public ShortZAxis(Axis axis, int board, int port)
			: base(axis, board, port)
		{
			Maximum = 43;
			StepsPerMillimeter = 533.33f;
			MaxSpeed = 2280;
			Driver = new DriverSpec(Label)
			{
				Direction = Direction.Forwards,
				Endstop = new EndstopSpec(Label)
				{
					SensorPort = $"{(board > 0 ? board : null)}.io{port}.in",
					Signal = PinSignal.Low,
					Location = EndstopLocation.LowEnd,
					FirstHomingSpeed = 600
				}
			};
		}
	}
}
