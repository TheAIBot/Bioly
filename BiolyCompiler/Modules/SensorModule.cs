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
            SensorModule sensor = new SensorModule();
            sensor.shape = new Rectangle(shape);
            return sensor;
        }
    }
}
