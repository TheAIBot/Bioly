using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using BiolyCompiler.Modules.OperationTypes;
using BiolyCompiler.Modules;

namespace BiolyCompiler.BlocklyParts.Blocks
{
    public abstract class Block
    {
        public readonly bool CanBeOutput;
        //For the scheduling:
        public Module boundedModule;
        public bool hasBeenScheduled = false;
        public int estimatedLongestPath = Int32.MaxValue;

        public Block(bool canBeOutput)
        {
            this.CanBeOutput = canBeOutput;
        }

        public abstract Block TryParseBlock(XmlNode node);

        public virtual OperationType getOperationType(){
            return OperationType.Unknown;
        }

        public void Bind(Module module){
            boundedModule = module;
        }
    }
}
