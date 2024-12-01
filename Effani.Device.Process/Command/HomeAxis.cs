using Effani.Device.Control;
using System;
using System.Linq;

namespace Effani.Device.Process
{
	public class HomeAxis(Axis axis, DeviceControl device) : Command(device)
	{
		public override void Run()
		{
			Result = Device.Home(_axis);
		}
		private readonly Axis _axis = axis;
	}
}
