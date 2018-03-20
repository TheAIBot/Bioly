using System;
using System.Drawing;
using BiolyCompiler.Modules.OperationTypes;
using BiolyCompiler.Scheduling;

namespace BiolyCompiler.Modules
{
    public abstract class Module
    {
        public Rectangle shape;
        public Route routeToModule;
        public int operationTime;
        int numberOfInputs, numberOfOutputs;
        
        public Module(int width, int height, int operationTime){
            shape = new Rectangle(width, height);
            this.operationTime = operationTime;
            shape.isEmpty = false;
        }
        public Module(int width, int height, int operationTime, int numberOfInputs, int numberOfOutputs) : this(width, height, operationTime)
        {
            this.numberOfInputs  = numberOfInputs;
            this.numberOfOutputs = numberOfOutputs;
        }


        public abstract OperationType getOperationType();

        public override String ToString()
        {
            return this.GetType().ToString() + ", dimensions = " + shape.ToString() + ", operation time = " + operationTime;
        }

        //Returns a copy of the module (not taking adjacencies into account). 
        //It is used for creating unique modules, for the binding process in the scheduling
        public abstract Module GetCopyOf();

        //True iff the module is placed permenently on the board.
        public virtual bool isStaticModule()
        {
            return false;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            Module moduleObj = obj as Module;
            if (moduleObj == null) return false;
            else if (moduleObj.GetType() != this.GetType()) return false;
            else return shape.Equals(moduleObj.shape) && 
                        numberOfInputs == moduleObj.numberOfInputs && 
                        numberOfOutputs == moduleObj.numberOfOutputs;
        }
    }
}
