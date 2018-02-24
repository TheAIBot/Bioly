using System;
using BiolyCompiler.Modules.OperationTypes;

namespace BiolyCompiler.Modules
{
    public class Sensor : Module
    {

        public Sensor() : base(3, 3, 3000){
        }


        public override OperationType getOperationType(){
            return OperationType.Sensor;
        }
    }
}
