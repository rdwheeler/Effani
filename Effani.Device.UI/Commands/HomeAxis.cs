using Effani.Device.Control;
using System;
using System.Linq;

namespace Effani.Device.Process
{
	public class HomeAxis(Axis axis, AbalDeviceControl device) : Home(device)
	{
		protected override void Run()
		{
			AxisSpec = (Device as AbalDeviceControl).Axes[axis];

			base.Run();
		}

		private readonly Axis _axis = axis;
	}
}
