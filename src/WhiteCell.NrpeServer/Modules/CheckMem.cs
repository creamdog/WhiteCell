using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace WhiteCell.NrpeServer.Modules
{
    public class CheckMem : IModule
    {
        public string Execute(IEnumerable<string> arguments)
        {
            PerformanceCounter ramCounter;
            ramCounter = new PerformanceCounter("Memory", "Available MBytes");
            return ramCounter.NextValue().ToString();
        }
    }
}
