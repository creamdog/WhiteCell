using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using WhiteCell.NrpeServer.Extensions;
using log4net;

namespace WhiteCell.NrpeServer
{
    public class Server
    {
        public IPAddress LocalAddress { get; set; }
        public int Port { get; set; }


        private ILog Log = LogManager.GetLogger(typeof(Server));
        private TcpListener tcpListener;
        private Thread listenThread;
        private List<Thread> clientThreadPool;
        private const int MAX_BUFFER_LENGTH = 1024;
        private const int MAX_CLIENT_COUNT = 10;
        private const string MAX_CLIENT_COUNT_EXCEEDED = "MAX_CLIENT_COUNT_EXCEEDED";
        private AutoResetEvent connectionWaitHandle = new AutoResetEvent(false);


        public void Start()
        {
            tcpListener = new TcpListener(LocalAddress, Port);
            
            listenThread = new Thread(new ThreadStart(AcceptConnections));
            listenThread.Start();
        }

        public void Stop()
        {
            Log.Info("Shutting down server...");
            listenThread.Abort();
        }

        private void CleanClientThreadPool()
        {
            if (clientThreadPool == null)
                return;

            var deadThreads = clientThreadPool.Where(thread => !thread.IsAlive).ToList();

            if (deadThreads.Count > 0)
                Log.InfoFormat("removing {0} dead threads from clientThreadPool", deadThreads.Count);

            foreach (var thread in deadThreads)
                clientThreadPool.Remove(thread);
        }

        private void AcceptConnections()
        {
            try
            {
                Log.Info("Starting server");

                clientThreadPool = new List<Thread>();
                tcpListener.Start();

                while (true)
                {
                    if (Thread.CurrentThread.ThreadState != ThreadState.Running)
                        break;

                    CleanClientThreadPool();

                    tcpListener.BeginAcceptTcpClient(HandleAsyncConnection, tcpListener);
                    connectionWaitHandle.WaitOne();
                }
            }
            finally
            {
                tcpListener.Stop();
                ShutDownClientThreadPool();
            }
        }

        private void HandleAsyncConnection(IAsyncResult result)
        {

            TcpListener listener = (TcpListener)result.AsyncState;
            TcpClient client = listener.EndAcceptTcpClient(result);

            connectionWaitHandle.Set();

            if (clientThreadPool.Count >= MAX_CLIENT_COUNT)
            {
                Log.InfoFormat("length of clientThreadPool exceeds MAX_CLIENT_COUNT {0}", MAX_CLIENT_COUNT);

                client.GetStream().Write(ASCIIEncoding.ASCII.GetBytes(MAX_CLIENT_COUNT_EXCEEDED), 0, ASCIIEncoding.ASCII.GetBytes(MAX_CLIENT_COUNT_EXCEEDED).Length);
                client.Close();
                return;
            }

            Log.Info("Client connected");

            Thread clientThread = new Thread(new ParameterizedThreadStart(HandleClientRequest));
            clientThreadPool.Add(clientThread);
            clientThread.Start(client);
        }

        private void ShutDownClientThreadPool()
        {
            Log.InfoFormat("Aborting {0} threads in clientThreadPool", clientThreadPool.Count);

            foreach (var thread in clientThreadPool.Where(thread => thread.IsAlive))
                thread.Abort();
        }

        private void HandleClientRequest(object clientObject)
        {
            TcpClient tcpClient = clientObject as TcpClient;

            try
            {
                if (tcpClient == null)
                    return;

                Log.InfoFormat("Accepted client connection from {0}", tcpClient.Client.RemoteEndPoint);

                NetworkStream clientStream = tcpClient.GetStream();

                byte[] message = new byte[MAX_BUFFER_LENGTH];

                var byteLen = clientStream.Read(message, 0, message.Length);
                
                if (byteLen == 0)
                    return;

                string command = ASCIIEncoding.ASCII.GetString(message.Take(byteLen).ToArray());

                Log.InfoFormat("executing command: {0}", command);

                string result = ExecuteCommand(command);

                Log.InfoFormat("command result: {0}", result);

                clientStream.Write(ASCIIEncoding.ASCII.GetBytes(result), 0, ASCIIEncoding.ASCII.GetBytes(result).Length);

            }
            catch (ThreadAbortException)
            {
                throw;
            }
            catch (Exception e)
            {
                Log.Error(e.Message, e);
                throw;
            }
            finally
            {
                if(tcpClient != null)
                    tcpClient.Close();
            }
        }

        private string ExecuteCommand(string commandLine)
        {
            if (commandLine == null)
                throw new NullReferenceException("command may not be null");

            var arguments = new List<string>(commandLine.Split('&'));

            if(arguments.Count < 2)
                throw new ArgumentException("invalid number of arguments");

            string password = arguments.First();
            arguments.RemoveAt(0);
            string command = arguments.First();
            arguments.RemoveAt(0);

            var types = typeof(Server).Assembly.GetTypes();// AppDomain.CurrentDomain.GetAssemblies().Where(a => a.GetReferencedAssemblies().Where(aa => aa.FullName == typeof(Server).Assembly.FullName).Count() > 0).GetTypes();

            foreach (var type in from t in types where t.GetInterfaces().Contains(typeof(IModule)) select t)
            {
                if (type.Name != command)
                    continue;

                var module = (IModule)Activator.CreateInstance(type);

                return module.Execute(arguments);
            }

            return string.Format("command {0} not found", command);
        }
    }
}
