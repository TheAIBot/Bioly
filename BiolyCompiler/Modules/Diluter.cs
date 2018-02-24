using System;
using BiolyCompiler.Modules.OperationTypes;

namespace BiolyCompiler.Modules
{
    public class Diluter : Module
    {

        public Diluter(int height, int width, int operationTime) : base(height, width, operationTime){
            
        }

        public override OperationType getOperationType(){
            return OperationType.Diluter;
        }
    }
}
