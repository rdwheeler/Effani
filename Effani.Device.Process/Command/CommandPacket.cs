using System;
using System.Linq;

namespace Effani.Device.Process
{
	public class CommandPacket(Command[] commands)
	{
		public Command[] Commands { get; } = commands;
	}
}
