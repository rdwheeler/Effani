using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Drivetrain.Robotics.Control
{
	public class AxisSpec : Spec
	{
		public AxisSpec(Axis axis, int board, int port, decimal? startingPosition = null)
			: base(axis.GetAttribute<DisplayAttribute>().Name)
		{
			Axis = axis;
			DriverPort = $"{board}.{port}";
			Designation = axis.GetAttribute<DisplayAttribute>().ShortName;
			StartingPosition = startingPosition ?? 0;
		}
		public string? Designation { get; set; }
		public string? DriverPort { get; set; }
		public int Minimum { get; set; }
		public int Maximum { get; set; }
		public decimal StartingPosition { get; set; }
		public decimal StepsPerMillimeter { get; set; }

		public int MaxSpeed { get; set; } = 500;

		public int Acceleration { get; set; } = 500;
		public DriverSpec Driver { get; set; }
		public Axis Axis { get; set; }
	}
}
