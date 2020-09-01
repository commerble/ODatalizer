using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace ODatalizer.EFCore
{
    public class ODatalizerLogEvents
    {
        public static EventId ControllerCodeGenerated { get; set; } = new EventId(90000, "ControllerCodeGenerated");
        public static EventId ControllerCodeCompileFaild { get; set; } = new EventId(90001, "ControllerCodeCompileFaild");
        public static EventId DynamicVisitorNullReferenced { get; set; } = new EventId(90002, "DynamicVisitorNullReferenced");
    }
}
