using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ClientConsole.Commands
{
	public interface ITodoCommand
	{
		IList<string> GetKeys(); 
		void Execute(string raw);
	}
}
