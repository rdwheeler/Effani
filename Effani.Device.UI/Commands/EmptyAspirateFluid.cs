using Effani.Device.Control;
using System;
using System.Linq;

namespace Effani.Device.Process
{
	public class EmptyAspirateFluid(AbalDeviceControl device) : EmptyAspirate(device)
	{
		protected override void Run()
        {
			AxisSpec = (Device as AbalDeviceControl).Axes[Axis.AspirateFluid] as AspirateAxis;

			base.Run();
        }
	}
}
