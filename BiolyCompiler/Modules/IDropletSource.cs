using BiolyCompiler.Routing;
using System;
using System.Collections.Generic;
using System.Text;

namespace BiolyCompiler.Modules
{
    public interface IDropletSource
    {
        BoardFluid getFluidType();

        (int,int) getMiddleOfSource();

        bool isInMiddleOfSource(RoutingInformation location);
    }
}
