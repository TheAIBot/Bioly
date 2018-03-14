using System;
using System.Xml;

namespace BiolyCompiler
{
    public class Compiler
    {
        public void DoStuff()
        {
            Parser.XMLParser.Parse("<xml xmlns=\"http://www.w3.org/1999/xhtml\"><variables><variable type=\"\" id=\"^:osIH6924)H-:?.K3!z\">input_fluid_name</variable><variable type=\"\" id=\")Z^HN_?}{]mnHOn-s*|/\">fluid_name</variable></variables><block type=\"start\" id=\"mx;$3h*?e?Xe`oWSMZC[\" x=\"116\" y=\"58\"><statement name=\"program\"><block type=\"input\" id=\"r;Sx1h=-=QW|KLB$98q[\"><field name=\"inputName\" id=\"^:osIH6924)H-:?.K3!z\" variabletype=\"\">input_fluid_name</field><field name=\"inputAmount\">undefined</field><field name=\"inputUnit\">drops</field><next><block type=\"fluid\" id=\"n$N2Re+)^Z:j^D7D5]|s\"><field name=\"fluidName\" id=\")Z^HN_?}{]mnHOn-s*|/\" variabletype=\"\">fluid_name</field><value name=\"inputFluid\"><block type=\"getInput\" id=\"(e+A7|(*xQ=%$_t9SuW?\"><field name=\"inputName\" id=\"^:osIH6924)H-:?.K3!z\" variabletype=\"\">input_fluid_name</field></block></value><next><block type=\"controls_repeat_ext\" id=\"P,n@Z[(/vv757oEuWzKz\"><value name=\"TIMES\"><block type=\"math_number\" id=\"9%V`pGWsFT!hi9zT)+5Z\"><field name=\"NUM\">19</field></block></value><statement name=\"DO\"><block type=\"fluid\" id=\"[:l/Cj?X94Eff%15O.ia\"><field name=\"fluidName\" id=\")Z^HN_?}{]mnHOn-s*|/\" variabletype=\"\">fluid_name</field><value name=\"inputFluid\"><block type=\"split\" id=\"_a|Pa}=v/H$F(1)Ns@:h\"><field name=\"fluidAmount\">0</field><value name=\"inputFluid\"><block type=\"getInput\" id=\"Tz2]7^ZHl[HV=j@c@72n\"><field name=\"inputName\" id=\"^:osIH6924)H-:?.K3!z\" variabletype=\"\">input_fluid_name</field></block></value></block></value><next><block type=\"fluid\" id=\"Iv-TF#0CF;Hz6B~:YD6O\"><field name=\"fluidName\" id=\")Z^HN_?}{]mnHOn-s*|/\" variabletype=\"\">fluid_name</field><value name=\"inputFluid\"><block type=\"heat\" id=\"MFryUG|cyUUOpeSH%GD9\"><field name=\"temperature\">0</field><field name=\"time\">0</field><value name=\"inputFluid\"><block type=\"getFluid\" id=\"uBtC9Fhp~;fszYYrzK3^\"><field name=\"fluidName\" id=\")Z^HN_?}{]mnHOn-s*|/\" variabletype=\"\">fluid_name</field></block></value></block></value></block></next></block></statement></block></next></block></next></block></statement></block></xml>");
        }
    }
}