using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ImageConvertWebServer
{
    internal class ClientContext
    {
        public TcpClient Client { get; }
        public string RootFolder { get; }

        public ClientContext(TcpClient client, string rootFolder)
        {
            Client = client;
            RootFolder = rootFolder;
        }
    }
}
