using BiolyCompiler.Exceptions.ParserExceptions;
using BiolyCompiler.Graphs;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using BiolyCompiler.BlocklyParts;

namespace BiolyCompiler.Parser
{
    public class ParserInfo
    {
        public readonly CDFG cdfg = new CDFG();
        public readonly List<ParseException> ParseExceptions = new List<ParseException>();
        public readonly HashSet<string> ValidModuleNames = new HashSet<string>();
        public readonly HashSet<string> ValidFluidVariableNames = new HashSet<string>();
        public readonly Stack<List<string>> FluidVariableScope = new Stack<List<string>>();
        public readonly HashSet<string> ValidNumberVariableNames = new HashSet<string>();
        public readonly Stack<List<string>> NumberVariableScope = new Stack<List<string>>();
        public readonly HashSet<string> ValidFluidArrayNames = new HashSet<string>();
        public readonly Stack<List<string>> FluidArrayScope = new Stack<List<string>>();
        public Dictionary<string, string> MostRecentVariableRef;
        private int Unique = 0;

        public void EnterDFG()
        {
            FluidVariableScope.Push(new List<string>());
            NumberVariableScope.Push(new List<string>());
            FluidArrayScope.Push(new List<string>());
            MostRecentVariableRef = new Dictionary<string, string>();
        }

        public void LeftDFG()
        {
            List<string> fluidVariablesOutOfScope = FluidVariableScope.Pop();
            fluidVariablesOutOfScope.ForEach(x => ValidFluidVariableNames.Remove(x));

            List<string> numberVariablesOutOfScope = NumberVariableScope.Pop();
            numberVariablesOutOfScope.ForEach(x => ValidNumberVariableNames.Remove(x));

            List<string> fluidArraysOutOfScope = FluidArrayScope.Pop();
            fluidArraysOutOfScope.ForEach(x => ValidFluidArrayNames.Remove(x));

            MostRecentVariableRef = null;
        }

        public void CheckModuleVariable(string id, string moduleName)
        {
            if (!ValidModuleNames.Contains(moduleName))
            {
                ParseExceptions.Add(new ParseException(id, $"Module {moduleName} isn't previously defined."));
            }
        }

        public void CheckFluidVariable(string id, string variableName)
        {
            if (!ValidFluidVariableNames.Contains(variableName))
            {
                ParseExceptions.Add(new ParseException(id, $"Fluid variable {variableName} isn't previously defined."));
            }
        }

        public void CheckNumberVariable(string id, string variableName)
        {
            if (!ValidNumberVariableNames.Contains(variableName))
            {
                ParseExceptions.Add(new ParseException(id, $"Numeric variable {variableName} isn't previously defined."));
            }
        }

        public void CheckFluidArrayVariable(string id, string variableName)
        {
            if (!ValidFluidArrayNames.Contains(variableName))
            {
                ParseExceptions.Add(new ParseException(id, $"Fluid array {variableName} isn't previously defined."));
            }
        }

        public void AddModuleName(string moduleName)
        {
            ValidModuleNames.Add(moduleName);
        }

        public void AddFluidVariable(string variableName)
        {
            if (variableName == Block.DEFAULT_NAME)
            {
                return;
            }
            //fluid variable may have been declared in an earlier scope
            //if that's the case then don't add it to the new scope as well
            if (FluidVariableScope.All(x => !x.Contains(variableName)))
            {
                FluidVariableScope.Peek().Add(variableName);
            }
            ValidFluidVariableNames.Add(variableName);
        }

        public void AddNumberVariable(string variableName)
        {
            //the variable may have been declared in an earlier scope
            //if that's the case then don't add it to the new scope as well
            if (NumberVariableScope.All(x => !x.Contains(variableName)))
            {
                NumberVariableScope.Peek().Add(variableName);
            }
            ValidNumberVariableNames.Add(variableName);
        }

        public void AddFluidArrayVariable(string variableName)
        {
            //the variable may have been declared in an earlier scope
            //if that's the case then don't add it to the new scope as well
            if (FluidArrayScope.All(x => !x.Contains(variableName)))
            {
                FluidArrayScope.Peek().Add(variableName);
            }
            ValidFluidArrayNames.Add(variableName);
        }

        public string GetUniquePostFix()
        {
            Unique++;
            return $"{Validator.INLINE_PROGRAM_SPECIAL_SEPARATOR}{Unique}";
        }
    }
}
