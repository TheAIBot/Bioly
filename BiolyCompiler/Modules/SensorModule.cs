using System;
using BiolyCompiler.Modules.OperationTypes;

namespace BiolyCompiler.Modules
{
    public class SensorModule : Module
    {

        public SensorModule() : base(3, 3, 3000){
        }


        public override OperationType getOperationType(){
            return OperationType.Sensor;
        }

        public override Module GetCopyOf()
        {
            throw new NotImplementedException();
        }
    }
}
