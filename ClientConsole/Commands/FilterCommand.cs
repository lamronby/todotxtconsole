﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using ToDoLib;

namespace ClientConsole.Commands
{
    public class FilterCommand : ITodoCommand
    {
        private IList<string> _keys = new List<string> { "filter" };

        public string Description => "Add or remove filters. To remove the current filter, specify without any arguments.";

        public FilterCommand()
		{
		}

        public IList<string> GetKeys()
        {
            return _keys;
        }

        public void Execute(string commandArgs, CommandContext context)
        {
            if (String.IsNullOrEmpty(commandArgs))
            {
                if (context.Filter != null)
                    Console.WriteLine("Removed filter: {0}", context.Filter);

                context.Filter = null;
            }
            else
            {
                context.Filter = new TaskFilter(commandArgs);
                Console.WriteLine("Added filter: {0}", context.Filter);
            }
        }
    }
}
