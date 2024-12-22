using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Effani.Device.Control
{
	public class AxisSpec : Spec
	{
		public AxisSpec(Axis axis, int board, int port)
			: base(axis.ToString())
		{
			Axis = axis;
			DriverPort = $"{board}.{port}";
			Designation = axis.GetAttribute<DisplayAttribute>().ShortName;
		}
		public string? Designation { get; set; }
		public string? DriverPort { get; set; }
		public int Minimum { get; set; }
		public int Maximum { get; set; }
		public float StartingPosition { get; set; }
		public float StepsPerMillimeter { get; set; }

		public int MaxSpeed { get; set; } = 500;

		public int Acceleration { get; set; } = 500;
		public DriverSpec Driver { get; set; }
		public Axis Axis { get; set; }
	}
}
