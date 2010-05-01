using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WhiteCell.NrpeServer.Modules
{
    public class GetDiskNames : IModule
    {
        public string Execute(IEnumerable<string> arguments)
        {
            var driveNames = from drive in System.IO.DriveInfo.GetDrives() select drive.Name;
            return string.Join(",", driveNames);
        }
    }
}
