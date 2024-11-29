using System;
using System.Linq;

namespace Effani.Device.Control
{
	public class DeviceSpecification : Spec
	{
		public DeviceSpecification(string label) : base(label)
		{
			DeviceName = label;
		}

		public string DeviceName { get; set; }

		public List<AxisSpec> Axes { get; set; } = new();
	}
}
