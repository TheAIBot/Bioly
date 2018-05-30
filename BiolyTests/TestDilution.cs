using System;
using BiolyCompiler.Graphs;
using BiolyCompiler.Modules;
using BiolyCompiler.Scheduling;
using BiolyCompiler.Architechtures;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using BiolyCompiler.Modules.RectangleSides;
using System.Linq;
using BiolyCompiler.Routing;
using BiolyCompiler.BlocklyParts;
using BiolyCompiler.BlocklyParts.FFUs;
using BiolyCompiler.BlocklyParts.Sensors;
using BiolyTests.TestObjects;
using BiolyCompiler.BlocklyParts.Misc;
using System.IO;
using BiolyCompiler.Parser;
using BiolyCompiler.Exceptions.ParserExceptions;
using BiolyCompiler.BlocklyParts.Declarations;
using System.Diagnostics;
//using MoreLinq;

namespace BiolyTests.Dilution
{
    [TestClass]
    public class TestDilution
    {

        [TestMethod]
        public void testDMRW() {
            int[] mixingSequence = DMRW(0, 313 / (float)1024, 1, 1 / (float) 1024);

            //Initial left source
            Assert.AreEqual(0, mixingSequence[0]); //isAssignedLeft
            Assert.AreEqual(0, mixingSequence[1]); //Left child
            Assert.AreEqual(0, mixingSequence[2]); //Right child
            Assert.AreEqual(6, mixingSequence[3]); //Number of droplets required

            //Initial right source
            Assert.AreEqual(0, mixingSequence[4]); //isAssignedLeft
            Assert.AreEqual(0, mixingSequence[5]); //Left child
            Assert.AreEqual(0, mixingSequence[6]); //Right child
            Assert.AreEqual(3, mixingSequence[7]); //Number of droplets required

            //2
            Assert.AreEqual(1, mixingSequence[8]); //isAssignedLeft
            Assert.AreEqual(0, mixingSequence[9]); //Left child
            Assert.AreEqual(1, mixingSequence[10]); //Right child
            Assert.AreEqual(5, mixingSequence[11]); //Number of droplets required

            //3
            Assert.AreEqual(0, mixingSequence[12]); //isAssignedLeft
            Assert.AreEqual(0, mixingSequence[13]); //Left child
            Assert.AreEqual(2, mixingSequence[14]); //Right child
            Assert.AreEqual(6, mixingSequence[15]); //Number of droplets required

            //4
            Assert.AreEqual(1, mixingSequence[16]); //isAssignedLeft
            Assert.AreEqual(3, mixingSequence[17]); //Left child
            Assert.AreEqual(2, mixingSequence[18]); //Right child
            Assert.AreEqual(3, mixingSequence[19]); //Number of droplets required

            //5
            Assert.AreEqual(1, mixingSequence[20]); //isAssignedLeft
            Assert.AreEqual(3, mixingSequence[21]); //Left child
            Assert.AreEqual(4, mixingSequence[22]); //Right child
            Assert.AreEqual(5, mixingSequence[23]); //Number of droplets required

            //6
            Assert.AreEqual(0, mixingSequence[24]); //isAssignedLeft
            Assert.AreEqual(3, mixingSequence[25]); //Left child
            Assert.AreEqual(5, mixingSequence[26]); //Right child
            Assert.AreEqual(1, mixingSequence[27]); //Number of droplets required

            //7
            Assert.AreEqual(0, mixingSequence[28]); //isAssignedLeft
            Assert.AreEqual(6, mixingSequence[29]); //Left child
            Assert.AreEqual(5, mixingSequence[30]); //Right child
            Assert.AreEqual(2, mixingSequence[31]); //Number of droplets required

            //8
            Assert.AreEqual(0, mixingSequence[32]); //isAssignedLeft
            Assert.AreEqual(7, mixingSequence[33]); //Left child
            Assert.AreEqual(5, mixingSequence[34]); //Right child
            Assert.AreEqual(3, mixingSequence[35]); //Number of droplets required

            //9
            Assert.AreEqual(1, mixingSequence[36]); //isAssignedLeft
            Assert.AreEqual(8, mixingSequence[37]); //Left child
            Assert.AreEqual(5, mixingSequence[38]); //Right child
            Assert.AreEqual(1, mixingSequence[39]); //Number of droplets required

            //10
            Assert.AreEqual(1, mixingSequence[40]); //isAssignedLeft
            Assert.AreEqual(8, mixingSequence[41]); //Left child
            Assert.AreEqual(9, mixingSequence[42]); //Right child
            Assert.AreEqual(1, mixingSequence[43]); //Number of droplets required

            //11
            Assert.AreEqual(1, mixingSequence[44]); //isAssignedLeft
            Assert.AreEqual(8, mixingSequence[45]); //Left child
            Assert.AreEqual(10, mixingSequence[46]); //Right child
            Assert.AreEqual(1, mixingSequence[47]); //Number of droplets required
            
        }


        public int[] DMRW(float Cl, float Ch, float Ct, float toleratedError)
        {
            float LeftBoundary = Cl;
            float RightBoundary = Ct;
            float MiddleValue = (LeftBoundary + RightBoundary) / 2;
            float error = 1;
            int groupElements = 4;
            int indexNumberOfDroplets = 3;
            int leftChildPosition = 0;
            int rightChildPosition = 1;
            //Groups of three numbers -> (isAssignedLeft, LeftChildIndex, RightChildIndex, numberOfDroplets)
            int[] mixingSequence = new int[groupElements * 100];
            
            int NumOfSteps = 1;
            while (error >= toleratedError)
            {
                //New iteration
                NumOfSteps = NumOfSteps + 1;
                MiddleValue = (LeftBoundary + RightBoundary) / 2;
                Debug.WriteLine(MiddleValue * 1024);
                //Calculating error
                if (Ch - MiddleValue > 0)
                    error = Ch - MiddleValue;
                else
                    error = MiddleValue - Ch;

                //For backtracking:
                mixingSequence[NumOfSteps * groupElements + 1] = leftChildPosition;
                mixingSequence[NumOfSteps * groupElements + 2] = rightChildPosition;

                if (MiddleValue < Ch)
                {
                    mixingSequence[NumOfSteps * groupElements + 0] = 0;
                    LeftBoundary = MiddleValue;
                    leftChildPosition = NumOfSteps;
                }
                else
                {
                    RightBoundary = MiddleValue;
                    mixingSequence[NumOfSteps * groupElements + 0] = 1;
                    rightChildPosition = NumOfSteps;
                }

            }
            int totalNumberOfSteps = NumOfSteps;


            //BackTracking:
            mixingSequence[NumOfSteps * groupElements + indexNumberOfDroplets] = 1; //1 droplet of the end result is required.
            while (NumOfSteps >= 2)
            {
                int currentAssignedLeft = mixingSequence[NumOfSteps * groupElements + 0];
                int LeftChildIndex = mixingSequence[NumOfSteps * groupElements + 1];
                int RightChildIndex = mixingSequence[NumOfSteps * groupElements + 2];
                float numberOfDroplets = mixingSequence[NumOfSteps * groupElements + indexNumberOfDroplets];
                int requiredNumberOfDropletsForMixing = (int) Math.Ceiling(numberOfDroplets / 2.0);

                //Updating left and right child number of required droplets
                mixingSequence[LeftChildIndex  * groupElements + indexNumberOfDroplets] += requiredNumberOfDropletsForMixing;
                mixingSequence[RightChildIndex * groupElements + indexNumberOfDroplets] += requiredNumberOfDropletsForMixing;

                NumOfSteps--;
            }
            Console.WriteLine("Kage");
            NumOfSteps++;



            //Now for the mixing:
            float leftFluid = 0;
            float rightFluid = 0;
            int numLeftFluid  = mixingSequence[0 * groupElements + indexNumberOfDroplets];
            int numRightFluid = mixingSequence[1 * groupElements + indexNumberOfDroplets];
            int mixedFluid = 0;
            while(NumOfSteps <= totalNumberOfSteps)
            {
                int i = 1;
                int numDropletsToMix = (int) Math.Ceiling(mixingSequence[NumOfSteps * groupElements + indexNumberOfDroplets]/2.0);
                float MixedFluid = mix(leftFluid, rightFluid);
                while (i < numDropletsToMix)
                {
                    float extraFluid = mix(leftFluid,rightFluid);
                    mixedFluid = union(extraFluid, (int) MixedFluid);
                }
                if (mixingSequence[NumOfSteps * groupElements + 0] == 0)
                {
                    leftFluid = mixedFluid;
                    numLeftFluid = mixedFluid;
                }
                else
                {
                    rightFluid = mixedFluid;
                    numRightFluid = mixedFluid;

                }
                NumOfSteps++;
            }

            //Output 1 droplet of mixedFluid

            return mixingSequence;
        }

        private int union(float fluid1, int fluid2)
        {
            return fluid2 + 2;
        }

        private int mix(float leftFluid, float rightFluid)
        {
            return (int) (leftFluid + rightFluid) / 2;
        }
        
    }
}
