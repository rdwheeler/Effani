using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Effani.Device.Control
{
	public abstract class Command(Device device)
	{
		public abstract void Execute();
		public Device Device { get; } = device;
		public string Result { get; protected set; }
	}
	public class EmitFluidCommand(Device device, Axis axis, int microLiters) : Command(device)
	{
		public override void Execute()
		{
			Result = Device.EmitFluid(Axis, MicroLiters);
		}
		public Axis Axis { get; } = axis;
		public int MicroLiters { get; } = microLiters;
	}
}
