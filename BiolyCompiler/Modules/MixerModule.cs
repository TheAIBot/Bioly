using System;
using BiolyCompiler.Modules.OperationTypes;


namespace BiolyCompiler.Modules
{
    public class MixerModule : Module
    {

        public MixerModule(int height, int width, int operationTime) : base(height, width, operationTime){

        }


        public override OperationType getOperationType(){
            return OperationType.Mixer;
        }

    }
}
