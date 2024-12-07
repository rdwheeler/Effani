using System;
using System.Linq;

namespace Effani.Device.Process
{
	public class CommandPacket(Command[] commands = null)
	{
		public Command[] Commands { get; } = commands;
	}
}
