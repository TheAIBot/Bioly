using System;
using System.Collections.Generic;
using System.Text;

namespace BiolyCompiler.Commands
{
    public enum CommandType : byte
    {
        ELECTRODE_ON,
        ELECTRODE_OFF,
        SHOW_AREA,
        REMOVE_AREA,
        SENSOR_START,
        SENSOR_STOP,
        SET_HEATER,
        START_EXECUTING_BLOCK,
        STOP_EXECUTING_BLOCK
    }
}
