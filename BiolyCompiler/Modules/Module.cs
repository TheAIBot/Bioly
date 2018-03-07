using System;
using System.Drawing;
using BiolyCompiler.Modules.OperationTypes;

namespace BiolyCompiler.Modules
{
    public abstract class Module
    {
        public Rectangle shape;
        public Point placement;
        public int operationTime;
        
        public Module(int height, int width, int operationTime){
            shape = new Rectangle(width, height);
            this.operationTime = operationTime;
        }

    

        public abstract OperationType getOperationType();
    }
}
