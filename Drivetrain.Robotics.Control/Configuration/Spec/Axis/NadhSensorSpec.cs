using System;
using System.Linq;

namespace Drivetrain.Robotics.Control
{
	public class NadhSensorSpec : SensorSpec
	{
		public NadhSensorSpec(string label, int sensorNumber, int board, int port, int readHigh, int readLow, Coordinate location = null)
			: base(label, sensorNumber, board, port, readHigh, readLow, location)
		{
		}
	}
}
