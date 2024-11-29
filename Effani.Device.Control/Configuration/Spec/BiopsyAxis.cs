using System;
using System.Linq;

namespace Effani.Device.Control
{
	public class BiopsyAxis : AxisSpec
	{
		public BiopsyAxis(Axis axis, int board, int port)
			: base(axis, board, port)
		{
			Maximum = 200;
			StepsPerMillimeter = 533.33f;
			MaxSpeed = 2280;
			Driver = new DriverSpec(Label)
			{
				Current = 1500,
				Direction = Direction.Backwards,
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
