using System;
using BiolyCompiler.Modules.OperationTypes;


namespace BiolyCompiler.Modules
{
    public class MixerModule : Module
    {

        public MixerModule(int width, int height, int operationTime) : base(width, height, operationTime){

        }


        public override OperationType getOperationType(){
            return OperationType.Mixer;
        }

        public override Module GetCopyOf()
        {
            throw new NotImplementedException();
        }
    }
}
