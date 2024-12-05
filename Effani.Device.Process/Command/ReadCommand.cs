using Effani.Device.Control;
using System;
using System.Linq;

namespace Effani.Device.Process
{
	public abstract class ReadCommand(DeviceControl device) : Command(device)
	{
	}
}
