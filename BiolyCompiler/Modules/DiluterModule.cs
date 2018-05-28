using System;
using System.Collections.Generic;
using BiolyCompiler.Commands;

namespace BiolyCompiler.Modules
{
    public class DiluterModule : Module
    {

        public DiluterModule(int height, int width, int operationTime) : base(height, width, operationTime, true){
            
        }

        public override Module GetCopyOf()
        {
            throw new NotImplementedException();
        }

        public override List<Command> GetModuleCommands(ref int time)
        {
            throw new NotImplementedException();
        }
    }
}
