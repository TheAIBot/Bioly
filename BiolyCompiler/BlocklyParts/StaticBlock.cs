﻿using BiolyCompiler.BlocklyParts.Misc;
using BiolyCompiler.Modules;
using System;
using System.Collections.Generic;
using System.Text;

namespace BiolyCompiler.BlocklyParts
{
    public abstract class StaticBlock : FluidBlock
    {
        //Each static block gets one associated module name. It must be uniqueue.
        //It will be used to find the associated module, during the scheduling.
        public const string MODULE_NAME_FIELD_NAME = "moduleName";
        public readonly string ModuleName; 

        public StaticBlock(string moduleName, bool canBeOutput, string output) : base(canBeOutput, output)
        {
            this.ModuleName = moduleName;
        }

        public StaticBlock(string moduleName, List<FluidInput> inputs, bool canBeOutput, string output) : base(canBeOutput, inputs, output)
        {
            this.ModuleName = moduleName;
        }

    }
}