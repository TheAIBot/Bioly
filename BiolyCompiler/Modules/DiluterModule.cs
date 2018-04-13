using System;
using System.Collections.Generic;
using BiolyCompiler.Commands;
using BiolyCompiler.Modules.OperationTypes;

namespace BiolyCompiler.Modules
{
    public class DiluterModule : Module
    {

        public DiluterModule(int height, int width, int operationTime) : base(height, width, operationTime, true){
            
        }

        public override OperationType getOperationType(){
            return OperationType.Diluter;
        }

        public override Module GetCopyOf()
        {
            throw new NotImplementedException();
        }

        protected override List<Command> GetModuleCommands()
        {
            throw new NotImplementedException();
        }
    }
}
