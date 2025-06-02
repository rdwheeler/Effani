using Drivetrain.Robotics.Control;
using System;
using System.Linq;
using Drivetrain.Robotics.Process;

namespace Effani.Device.Control
{
	public class HomeAxis : Home
	{
		private readonly Axis _axis;

		public HomeAxis(Axis axis, AbalDeviceControl device) : base(device)
		{
			_axis = axis;
			AxisSpec = (Device as AbalDeviceControl).Axes[_axis];
		}
	}
}
