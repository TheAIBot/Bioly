using System;
using System.Collections.Generic;
using System.Drawing;
using BiolyCompiler.BlocklyParts;
using BiolyCompiler.Modules.OperationTypes;
using BiolyCompiler.Scheduling;

namespace BiolyCompiler.Modules
{
    public abstract class Module
    {
        public Rectangle shape;
        public int operationTime;
        public Block bindingOperation;
        public readonly int numberOfInputs, numberOfOutputs;
        //The key is the input fluid name, see the operation/block which the module is bound to.
        public Dictionary<string, Route> InputRoutes = new Dictionary<string, Route>();
        
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
            //TODO Can be improved.
            return shape.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            Module moduleObj = obj as Module;
            if (moduleObj == null) return false;
            else if (moduleObj.GetType() != this.GetType()) return false;
            else
            {
                bool sameBindingOperation = (bindingOperation != null && moduleObj.bindingOperation != null && bindingOperation.Equals(moduleObj.bindingOperation)) ||
                                            (bindingOperation == null && moduleObj.bindingOperation == null);
                return shape.Equals(moduleObj.shape) &&
                        operationTime == moduleObj.operationTime &&
                        numberOfInputs == moduleObj.numberOfInputs &&
                        numberOfOutputs == moduleObj.numberOfOutputs &&
                        sameBindingOperation;
            }
        }

        public bool Implements(Block operation)
        {
            return  numberOfInputs  == operation.InputVariables.Count && 
                    //numberOfOutputs == operation.OutputVariable.Count &&
                    this.GetType().Equals(operation.getAssociatedModule().GetType());
        }
    }
}
