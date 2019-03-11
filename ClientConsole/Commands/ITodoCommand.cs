using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ClientConsole.Commands
{
	public interface ITodoCommand
	{
		IList<string> GetKeys();

        string Description { get; }

        void Execute(string commandArgs, CommandContext context);
	}
}
