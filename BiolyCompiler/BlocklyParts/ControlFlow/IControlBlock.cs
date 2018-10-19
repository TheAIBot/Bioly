using BiolyCompiler.Commands;
using BiolyCompiler.Graphs;
using BiolyCompiler.Modules;
using System;
using System.Collections.Generic;
using System.Text;

namespace BiolyCompiler.BlocklyParts.ControlFlow
{
    public interface IControlBlock : IEnumerable<DFG<Block>>
    {
        DFG<Block> GuardedDFG<T>(Dictionary<string, float> variables, CommandExecutor<T> executor, Dictionary<string, BoardFluid> dropPositions);
        DFG<Block> NextDFG<T>(Dictionary<string, float> variables, CommandExecutor<T> executor, Dictionary<string, BoardFluid> dropPositions);
        DFG<Block> TryLoop<T>(Dictionary<string, float> variables, CommandExecutor<T> executor, Dictionary<string, BoardFluid> dropPositions);
        IControlBlock Copy(DFG<Block> dfg, Dictionary<DFG<Block>, DFG<Block>> knownDFGCopys);
    }
}