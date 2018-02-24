using System;
using BiolyCompiler.Modules.OperationTypes;

namespace BiolyCompiler.Modules
{
    public abstract class Module
    {
        public Rectangle area;
        public uint operationTime;
        
        public Module(int height, int width, int operationTime){

        }

        public abstract OperationType getOperationType();
    }
}
