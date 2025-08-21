using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ImageConvertWebServer
{
    internal class ImageServer
    {
        private readonly int _port;
        private readonly string _rootFolder;
        private TcpListener _listener;
        private bool _isRunning;


        public ImageServer(int port, string rootFolder) {
            _port = port;
            _rootFolder = rootFolder;
        }

        public void Start()
        {
            // Kreiramo novi listener za tcpKonekciju i pokrenemo ga da osluskuje - IPAddress.Any = olsukuje na svim ip adresama koje ima u mrezi, 0.0.0.0, 127.0. itd
            _listener = new TcpListener(IPAddress.Any, _port);
            _listener.Start();
            _isRunning = true;

            //Logujemo podatke 
            Logger.LogInfo($"Server started at port {_port}");

            //Pokrecemo funckiju ListenLoop koja se vrti sve dok je server u running stanju i ceka da pokupi klijenta
            //Dodeljujemo funkciji posebnu nit iz ThreadPool
            ThreadPool.QueueUserWorkItem(ListenLoop);
        }

        public void Stop()
        {
            _isRunning = false;
			if (_listener != null)
			{
				_listener.Stop();
				Logger.LogInfo("Server stopped");
			}
			else
				Logger.LogInfo("Stop called, but listener was null");
		}

        private void ListenLoop(object state)
        {
            while (_isRunning)
            {
                try
                {
                    //Pokusavamo da prihvatimo klijenta - AcceptTcpClient je blokirajuca funkcija (ceka klijenta da se poveze pa onda ide dalje)
                    var client = _listener.AcceptTcpClient();
                    Logger.LogInfo($"NEW CONNECTION: {((IPEndPoint)client.Client.RemoteEndPoint).Address}"); // vraca se klijentova IPadr i port pa se zapisuje samo IPadr

                    // Za svakog klijenta kreiramo novu nit koja ce da ga opsluzi
                    ThreadPool.QueueUserWorkItem(RequestHandler.HandleClient, new ClientContext(client, _rootFolder));
                }
                catch (SocketException ex)
                {
                    Logger.LogError("ERROR CONNECTION: " + ex.Message);
                }
            }
        }
    }
}
