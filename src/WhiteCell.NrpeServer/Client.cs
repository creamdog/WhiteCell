using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using log4net;

namespace WhiteCell.NrpeServer
{
    public class Client
    {
        private ILog Log = LogManager.GetLogger(typeof(Client));
        public string HostName { get; set; }
        public int Port { get; set; }

        private const int MAX_BUFFER_LENGTH = 1024;

        public string Execute(string command)
        {
            try
            {
                Log.InfoFormat("Executing command {0} @{1}:{2}", command, HostName, Port);

                var client = new TcpClient(HostName, Port);

                var commandBytes = ASCIIEncoding.ASCII.GetBytes(command);

                client.GetStream().Write(commandBytes, 0, commandBytes.Length);

                var buffer = new byte[MAX_BUFFER_LENGTH];

                var readLen = client.GetStream().Read(buffer, 0, buffer.Length);

                var result = ASCIIEncoding.ASCII.GetString(buffer.Take(readLen).ToArray());

                client.Close();

                return result;
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message, ex);
                throw;
            }
        }
    }
}
