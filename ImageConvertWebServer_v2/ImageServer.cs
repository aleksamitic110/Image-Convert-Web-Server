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

		public ImageServer(int port, string rootFolder)
		{
			_port = port;
			_rootFolder = rootFolder;
		}

		public async Task StartAsync()
		{
			_listener = new TcpListener(IPAddress.Any, _port);
			_listener.Start();
			_isRunning = true;

			await Logger.LogInfoAsync($"Server started at port {_port}");

			// Ne treba nam ThreadPool direktno. Pokrećemo asinhronu petlju.
			_ = ListenLoopAsync(); // _ => ne cekamo da se ovaj task zavrsi -> to se zove (fire and forget)
		}

		public async Task StopAsync()
		{
			_isRunning = false;
			if (_listener != null)
			{
				_listener.Stop();
				await Logger.LogInfoAsync("Server stopped");
			}
			else
				await Logger.LogInfoAsync("Stop called, but listener was null");
		}

		// Petlja je sada asinhrona
		private async Task ListenLoopAsync()
		{
			while (_isRunning)
			{
				try
				{
					// Asinhrono cekamo klijenta -> ne blokiramo nit
					var client = await _listener.AcceptTcpClientAsync();
					await Logger.LogInfoAsync($"NEW CONNECTION: {((IPEndPoint)client.Client.RemoteEndPoint).Address}");

					// Za svakog klijenta kreiramo i pokrećemo novi Task koji će ga obraditi umesto da mu dodelimo nit sa QueueUserWorkItem()
					_ = Task.Run(() => RequestHandler.HandleClientAsync(new ClientContext(client, _rootFolder)));
				}
				catch (Exception ex)
				{
					await Logger.LogErrorAsync("ERROR in listen loop: " + ex.Message);
				}
			}
		}
	}
}
