using System;
using BiolyCompiler.Modules.OperationTypes;

namespace BiolyCompiler.Modules
{
    public class DiluterModule : Module
    {

        public DiluterModule(int height, int width, int operationTime) : base(height, width, operationTime){
            
        }

        public override OperationType getOperationType(){
            return OperationType.Diluter;
        }
    }
}
