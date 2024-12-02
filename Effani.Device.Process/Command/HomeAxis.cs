using Effani.Device.Control;
using System;
using System.Linq;

namespace Effani.Device.Process
{
	public class HomeAxis(Axis axis, DeviceControl device) : Command(device)
	{
		protected override string Run()
		{
			Result = Device.Home(_axis);
			return Result;
		}

		public override string Result { get; protected set; } = $"Axis homed";

		private readonly Axis _axis = axis;
	}
}
