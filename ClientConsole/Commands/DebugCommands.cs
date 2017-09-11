using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace ClientConsole.Commands
{
    public class DebugCommands : ITodoCommand
    {
        private IList<string> _keys = new List<string> { "debug" };

        public IList<string> GetKeys()
        {
            return _keys;
        }

        public void Execute( string commandArgs, CommandContext context )
        {
            if ( commandArgs == "config" )
            {
                Console.WriteLine(context);
            }
        }
    }
}
