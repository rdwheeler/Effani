using System;
using System.Linq;

namespace Drivetrain.Robotics.Control
{
	public abstract class SensorSpec : Spec
	{
		public SensorSpec(string label, int sensorNumber, int board, int port, int readHigh, int readLow, Coordinate location = null)
			: base(label)
		{
			ReadHigh = readHigh;
			ReadLow = readLow;
			Board = board;
			Port = port;
			SensorNumber = sensorNumber;
			Location = location;
		}

		public string? PortDesignation
		{
			get
			{
				var boardDesignation = Board > 0 ? $"{Board}." : null;
				return $"{boardDesignation}io{Port}.in";
			}
		}
		public int Board { get; set; } 
		public int Port { get; set; }
		public int SensorNumber { get; set; }
		public int ReadHigh { get; set; }
		public int ReadLow { get; set; }
		public Coordinate? Location { get; set; }
	}
}
