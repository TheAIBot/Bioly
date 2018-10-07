using System;
using System.Collections.Generic;
using BiolyCompiler.Commands;
using BiolyCompiler.Exceptions;

namespace BiolyCompiler.Modules
{
    public class DiluterModule : Module
    {

        public DiluterModule(int height, int width, int operationTime) : base(height, width, operationTime, true){
            
        }

        public override List<Command> GetModuleCommands(ref int time)
        {
            throw new InternalRuntimeException("This method is not supported.");
        }
    }
}
