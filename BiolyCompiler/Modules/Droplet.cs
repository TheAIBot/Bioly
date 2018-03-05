using System;
using BiolyCompiler.Modules.OperationTypes;


namespace BiolyCompiler.Modules
{
    public class Droplet : Module
    {

        public Droplet() : base(3, 3, 0){

        }


        public override OperationType getOperationType(){
            return OperationType.DropletStorage;
        }

    }
}
