using System;
using System.Collections.Generic;
using System.Text;
using BiolyCompiler.Commands;

namespace BiolyCompiler.Modules
{
    public class OutputModule : Module
    {
        public OutputModule() : base(3, 3, 1, false)
        {
        }

        public override Module GetCopyOf()
        {
            return new OutputModule();
        }

        protected override List<Command> GetModuleCommands()
        {
            return new List<Command>();
        }
    }
}
