using BiolyCompiler.Routing;
using System;
using System.Collections.Generic;
using System.Text;

namespace BiolyCompiler.Modules
{
    public interface IDropletSource
    {
        BoardFluid GetFluidType();

        void SetFluidType(BoardFluid newFluidType);

        (int,int) GetMiddleOfSource();

        bool IsInMiddleOfSource(RoutingInformation location);
    }
}
