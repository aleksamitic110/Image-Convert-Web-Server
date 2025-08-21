using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ImageConvertWebServer
{
	internal class RequestHandler
	{
		// Metoda sada vraća Task, a naziv konvencionalno završava sa "Async"
		public static async Task HandleClientAsync(object state)
		{
			var context = (ClientContext)state;
			TcpClient client = context.Client;
			string rootFolder = context.RootFolder;

			using (client)
			using (var stream = client.GetStream())
			using (var reader = new StreamReader(stream, Encoding.ASCII))
			using (var writer = new StreamWriter(stream, Encoding.ASCII) { AutoFlush = true })
			{
				try
				{
					// Asinhrono čitanje sa mreže
					string requestLine = await reader.ReadLineAsync();
					if (string.IsNullOrEmpty(requestLine))
					{
						await Logger.LogErrorAsync("Primljen prazan zahtev.");
						return;
					}

					await Logger.LogRequestAsync(requestLine);

					string[] tokens = requestLine.Split(' ');
					if (tokens.Length != 3 || tokens[0] != "GET")
					{
						await Logger.LogErrorAsync("Nepoznat ili nevalidan HTTP metod.");
						await SendBadRequestAsync(writer);
						return;
					}

					string requestedPath = tokens[1].TrimStart('/');
					string requestedFilePath = Path.Combine(rootFolder, requestedPath);

					if (!File.Exists(requestedFilePath))
					{
						await Logger.LogErrorAsync($"Fajl nije pronađen: {requestedPath}");
						await SendNotFoundAsync(writer);
						return;
					}

					// Čekamo na rezultat iz CacheManagera
					byte[] pngData = await CacheManager.GetPngImageAsync(requestedFilePath);

					if (pngData == null)
					{
						await SendNotFoundAsync(writer);
						return;
					}

					await SendImageResponseAsync(writer, pngData);
					await Logger.LogInfoAsync($"Poslat PNG odgovor za fajl: {requestedPath}");
				}
				catch (Exception ex)
				{
					await Logger.LogErrorAsync("Greška pri obradi zahteva: " + ex.Message);
				}
			}
		}

		private static async Task SendImageResponseAsync(StreamWriter writer, byte[] imageData)
		{
			await writer.WriteLineAsync("HTTP/1.1 200 OK");
			await writer.WriteLineAsync("Content-Type: image/png");
			await writer.WriteLineAsync("Content-Length: " + imageData.Length);
			await writer.WriteLineAsync(); // Kraj headera
										   // Asinhrono pisanje sirovih bajtova u stream
			await writer.BaseStream.WriteAsync(imageData, 0, imageData.Length);
		}

		private static async Task SendNotFoundAsync(StreamWriter writer)
		{
			string body = "Nije nadjena slika";
			await writer.WriteLineAsync("HTTP/1.1 404 Not Found");
			await writer.WriteLineAsync("Content-Type: text/plain; charset=utf-8");
			await writer.WriteLineAsync("Content-Length: " + Encoding.UTF8.GetByteCount(body));
			await writer.WriteLineAsync();
			await writer.WriteLineAsync(body);
		}

		private static async Task SendBadRequestAsync(StreamWriter writer)
		{
			string body = "Nije nam stigao GET zahtev sa 3 parametra. Primer pravilnog zahteva:  GET /test.jpg HTTP/1.1";
			await writer.WriteLineAsync("HTTP/1.1 400 Bad Request");
			await writer.WriteLineAsync("Content-Type: text/plain; charset=utf-8");
			await writer.WriteLineAsync("Content-Length: " + Encoding.UTF8.GetByteCount(body));
			await writer.WriteLineAsync();
			await writer.WriteLineAsync(body);
		}
	}
}
