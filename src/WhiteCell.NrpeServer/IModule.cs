using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WhiteCell.NrpeServer
{
    public interface IModule
    {
        string Execute(IEnumerable<string> arguments);
    }
}
