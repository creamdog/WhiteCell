using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace WhiteCell.NrpeServer.Modules
{
    public class CheckCPU : IModule
    {
        public string Execute(IEnumerable<string> arguments)
        {
            PerformanceCounter cpuCounter; 
            
            cpuCounter = new PerformanceCounter(); 

            cpuCounter.CategoryName = "Processor"; 
            cpuCounter.CounterName = "% Processor Time"; 
            cpuCounter.InstanceName = "_Total";

            return cpuCounter.NextValue().ToString();

        }
    }
}
