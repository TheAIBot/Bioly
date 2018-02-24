using System;
using BiolyCompiler.Modules.OperationTypes;


namespace BiolyCompiler.Modules
{
    public class Mixer : Module
    {

        public Mixer(int height, int width, int operationTime) : base(height, width, operationTime){

        }


        public override OperationType getOperationType(){
            return OperationType.Mixer;
        }

    }
}
