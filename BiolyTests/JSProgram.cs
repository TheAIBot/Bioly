using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BiolyTests
{
    public class JSProgram
    {
        StringBuilder Builder = new StringBuilder();
        int nameID = 0;

        public void AddBlock(string name, string blockType)
        {
            Builder.Append($"const {name} = workspace.newBlock(\"{blockType}\");");
        }

        public void AddConnection(string inputBlockname, string inputName, string outputBlockName)
        {
            Builder.Append($"{inputBlockname}.getInput(\"{inputName}\").connection.connect({outputBlockName}.outputConnection);");
        }

        public void AddMixerSegment(string outputName, string inputNameA, string inputNameB)
        {
            JSProgram program = new JSProgram();
            string a = GetRandomName();
            string b = GetRandomName();
            string c = GetRandomName();
            string d = GetRandomName();
            program.AddBlock(a, "fluid");
            program.AddBlock(b, "mixer");
            program.AddBlock(c, "getInput");
            program.AddBlock(d, "getInput");
            program.AddConnection(a, "inputFluid", b);
            program.AddConnection(b, "inputFluidA", c);
            program.AddConnection(b, "inputFluidB", d);
        }

        public string GetRandomName()
        {
            nameID++;
            return "N" + nameID;
        }

        public override string ToString()
        {
            return Builder.ToString();
        }
    }
}
