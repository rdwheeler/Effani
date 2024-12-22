using System;
using System.Linq;

namespace Effani.Device.Process
{
	public class Job
	{
        public Job(String description, Command[]? commands = null)
        {
			Description = description;
			if (commands is not null && commands.Any())
                Commands.AddRange(commands);
        }

        public List<Command> Commands { get; set; } = new List<Command>();
		String Description { get; set; }
	}
}
