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
			Device.Home(new Axis[] { Axis.TableX, Axis.TableY, Axis.AspirateFluidZ, Axis.AspirateKohZ });
			Device.GoTo(Axis.AspirateFluid, Device.Configuration.AspirateFluidSyringeHomePosition);
			Device.GoTo(Axis.AspirateKoh, Device.Configuration.AspirateKohSyringeHomePosition);
			Device.GoTo(Axis.Biopsy, Device.Configuration.BiopsyHomePosition);
			Device.GoTo(Axis.BiopsyEject, Device.Configuration.BiopsyEjectHomePosition);
			Result = $"All axis homed";
			return Result;
		}

		public override string Result { get; protected set; } = $"All axis homed";
	}
}
