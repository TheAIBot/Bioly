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
        public readonly List<ParseException> parseExceptions = new List<ParseException>();
        public readonly HashSet<string> validModuleNames = new HashSet<string>();
        public readonly HashSet<string> validFluidVariableNames = new HashSet<string>();
        public readonly Stack<List<string>> fluidVariableScope = new Stack<List<string>>();
        public Dictionary<string, string> mostRecentVariableRef;

        public void EnterDFG()
        {
            fluidVariableScope.Push(new List<string>());
            mostRecentVariableRef = new Dictionary<string, string>();
        }

        public void LeftDFG()
        {
            List<string> variablesOutOfScope = fluidVariableScope.Pop();
            variablesOutOfScope.ForEach(x => validFluidVariableNames.Remove(x));

            mostRecentVariableRef = null;
        }

        public void CheckModuleVariable(string id, string moduleName)
        {
            if (!validModuleNames.Contains(moduleName))
            {
                parseExceptions.Add(new ParseException(id, $"Module {moduleName} isn't previously defined."));
            }
        }

        public void CheckFluidVariable(string id, string variableName)
        {
            if (!validFluidVariableNames.Contains(variableName))
            {
                parseExceptions.Add(new ParseException(id, $"Fluid variable {variableName} isn't previously defined."));
            }
        }

        public void AddModuleName(string moduleName)
        {
            validModuleNames.Add(moduleName);
        }

        public void AddFluidVariable(string variableName)
        {
            if (variableName == Block.DEFAULT_NAME)
            {
                return;
            }
            //fluid variable may have been declared in an earlier scope
            //if that's the case then don't add it to the new scope as well
            if (fluidVariableScope.All(x => !x.Contains(variableName)))
            {
                fluidVariableScope.Peek().Add(variableName);
            }
            validFluidVariableNames.Add(variableName);
        }
    }
}
