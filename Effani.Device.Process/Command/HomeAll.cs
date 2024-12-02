using Effani.Device.Control;
using System;
using System.Linq;

namespace Effani.Device.Process
{
	public class HomeAll(DeviceControl device) : Command(device)
	{

		protected override string Run()
		{
			foreach(var fluidLine in Device.FluidAxes)
			{
				Device.GoTo(fluidLine.Axis, Device.Configuration.FluidSyringeHomePosition);
			}
			Device.GoTo(Axis.AspirateFluid, Device.Configuration.AspirateFluidSyringeHomePosition);
			Device.Home(Axis.AspirateFluidZ);
			Device.GoTo(Axis.AspirateKoh, Device.Configuration.AspirateKohSyringeHomePosition);
			Device.Home(Axis.AspirateKohZ);
			Device.GoTo(Axis.Biopsy, Device.Configuration.BiopsyHomePosition);
			Result = $"All axis homed";
			return Result;
		}

		public override string Result { get; protected set; } = $"All axis homed";
	}
}
