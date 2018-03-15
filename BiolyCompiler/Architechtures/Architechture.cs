using System;
using System.Collections.Generic;
using BiolyCompiler.Modules;

namespace BiolyCompiler.Architechtures
{
    public class Architechture
    {
        //Dummy class for now.

        public int heigth, width;

        public Architechture(){

        }

        public bool canBePlaced(Module module){
            return false;
        }

        //Based on the algorithm seen in figure 6.3, "Fault-Tolerant Digital Microfluidic Biochips - Compilation and Synthesis"
        public bool place(Module module){
            return false;
        }
        
    }
}
