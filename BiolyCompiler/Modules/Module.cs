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
        
        public Module(int width, int height, int operationTime){
            shape = new Rectangle(width, height);
            this.operationTime = operationTime;
            shape.isEmpty = false;
        }

    

        public abstract OperationType getOperationType();

        public override String ToString()
        {
            return this.GetType().ToString() + ", dimensions = " + shape.ToString() + ", operation time = " + operationTime;
        }

        //Returns a copy of the module (not taking adjacencies into account). 
        //It is used for creating unique modules, for the binding process in the scheduling
        public abstract Module GetCopyOf();
    }
}
