using System;
using System.Linq;

namespace Drivetrain.Robotics.Control
{
	public class DeviceSpecification : Spec
	{
		public DeviceSpecification() : base("Effani Device")
		{
		}
		public string DeviceName { get; set; }

		public List<AxisSpec> Axes { get; set; } = new();
		public List<Plate> Plates { get; set; } = new();
	}
}
