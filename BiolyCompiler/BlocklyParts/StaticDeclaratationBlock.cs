using BiolyCompiler.BlocklyParts.Misc;
using BiolyCompiler.Modules;
using System;
using System.Collections.Generic;
using System.Text;

namespace BiolyCompiler.BlocklyParts
{
    public class StaticDeclarationBlock : StaticBlock
    {

        public StaticDeclarationBlock(string moduleName, bool canBeOutput, string output, string id) : base(moduleName, canBeOutput, output, id)
        {
        }
        

    }
}
