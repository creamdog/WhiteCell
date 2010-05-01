using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using System.Threading;
using System.Net;

namespace WhiteCell.NrpeServer.IntegrationTests
{
    public class Class1
    {

        [Fact]
        public void TestServerClient()
        {
            var server = new WhiteCell.NrpeServer.Server { LocalAddress = IPAddress.Any, Port = 8989 };
            var client = new WhiteCell.NrpeServer.Client { HostName="localhost", Port = 8989 };

            server.Start();

            var driveNames = client.Execute("password&GetDiskNames");
            Console.WriteLine("mapped drives are {0}", driveNames);

            var freeSpaceOnC = client.Execute("password&CheckDisk&C:\\");
            Console.WriteLine("free space on C:\\ is {0} bytes", freeSpaceOnC);

            var cpu = client.Execute("password&CheckCPU");
            Console.WriteLine("cpu usage {0}%", cpu);

            var mem = client.Execute("password&CheckMem");
            Console.WriteLine("free memory {0}MB", mem);

            server.Stop();
        }



    }
}
