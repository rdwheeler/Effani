using Drivetrain.Robotics.Control;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Drivetrain.Robotics.Process
{
	public abstract class Command(DeviceControl device)
	{
		public void Execute()
		{
			Run();
		}

		protected abstract void Run();

		public DeviceControl Device { get; } = device;
		public abstract string Description { get; }
		public string Result { get; set; }
	}
}
