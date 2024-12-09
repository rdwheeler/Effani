using System;
using System.Linq;

namespace Effani.Device.Process
{
	public class CommandPacket
	{
        public CommandPacket(Command[]? commands = null)
        {
            if (commands is not null && commands.Any())
                Commands.AddRange(commands);
        }

        public List<Command> Commands { get; set; } = new List<Command>();
	}
}
