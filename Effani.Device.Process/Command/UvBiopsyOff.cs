using Effani.Device.Control;
using System;
using System.Linq;

namespace Effani.Device.Process
{
	public class UvBiopsyOff(DeviceControl device) : Command(device)
	{
		protected override string Run()
		{
			Result = Device.BiopsyUvOff();
			return Result;
		}

		public override string Result { get; protected set; } = $"UV Biopsy off";
    }
}
