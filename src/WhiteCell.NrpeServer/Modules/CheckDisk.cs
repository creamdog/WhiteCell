using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WhiteCell.NrpeServer.Modules
{
    public class CheckDisk : IModule
    {
        public string Execute(IEnumerable<string> arguments)
        {
            var driveInfo = System.IO.DriveInfo.GetDrives().Where(drive => drive.Name == arguments.FirstOrDefault()).FirstOrDefault();
            return driveInfo == null ? "no disk found" : driveInfo.TotalFreeSpace.ToString();
        }
    }
}
