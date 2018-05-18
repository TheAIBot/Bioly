using BiolyCompiler.Commands;
using BiolyCompiler.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BiolyTests
{
    class TestCommandExecutor : CommandExecutor<string>
    {
        public List<Command> Commands = new List<Command>();

        public override void QueueCommands(List<Command> commands)
        {
            Commands.AddRange(commands);
        }

        public override void SendCommands()
        {
        }

        public override void StartExecutor(List<Module> inputs, List<Module> outputs)
        {
        }

        public override V WaitForResponse<V>()
        {
            throw new NotImplementedException();
        }

        protected override string ConvertCommand(List<Command> commands)
        {
            throw new NotImplementedException();
        }
    }
}
