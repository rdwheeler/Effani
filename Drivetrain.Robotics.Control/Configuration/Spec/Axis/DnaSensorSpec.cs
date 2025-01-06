using System;
using System.Linq;

namespace Drivetrain.Robotics.Control
{
	public class DnaSensorSpec : SensorSpec
	{
		public DnaSensorSpec(string label, int sensorNumber, int board, int port, int readHigh, int readLow, Coordinate location = null)
			: base(label, sensorNumber, board, port, readHigh, readLow, location)
		{
		}
	}
}
