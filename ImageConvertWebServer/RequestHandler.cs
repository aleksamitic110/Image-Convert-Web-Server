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
        public static void HandleClient(object state) // Object state mora zbog funkcije ThreadPool.QueueUserWorkItem(funkcija, object state - parametri funkcije)
        {
            var context = (ClientContext)state;
            TcpClient client = context.Client;
            string rootFolder = context.RootFolder;

            using (client)
            using (var stream = client.GetStream()) // Za komunikaciju  full duplex jer se radi o TCP konekciji / vraca NetworkStream
            using (var reader = new StreamReader(stream, Encoding.ASCII)) // Smer klijent -> server 
			using (var writer = new StreamWriter(stream, Encoding.ASCII) { AutoFlush = true }) // Smer server -> klijent
			{
                try
                {
                    // Pročitaj prvi red HTTP zahteva
                    string requestLine = reader.ReadLine(); // GET /test.jpg HTTP/1.1
					if (string.IsNullOrEmpty(requestLine))
                    {
                        Logger.LogError("Primljen prazan zahtev.");
                        return;
                    }

                    Logger.LogRequest(requestLine);

                    
                    string[] tokens = requestLine.Split(' ');
                    if (tokens.Length != 3 || tokens[0] != "GET")
                    {
                        Logger.LogError("Nepoznat ili nevalidan HTTP metod.");
                        SendBadRequest(writer); // Obavestava se klijent
                        return;
                    }

                    string requestedPath = tokens[1].TrimStart('/'); //    /test.jpg -> test.jpg
                    string requestedFilePath = Path.Combine(rootFolder, requestedPath);

                    // Proveri da li fajl postoji
                    if (!File.Exists(requestedFilePath))
                    {
                        Logger.LogError($"Fajl nije pronađen: {requestedPath}");
                        SendNotFound(writer); 
                        return;
                    }

					// GetPngImage pokusava da nadje sliku u CacheManager-u a ako je ne nadje, dodaje je u cache i vraca
					byte[] pngData = CacheManager.GetPngImage(requestedFilePath); 

                    if (pngData == null) // Ne bi trebalo nikad da se desi ali cisto za svaki slucaj dupla provera da li slika uopste nadjena/postoji
                    {
                        SendNotFound(writer);
						return;
                    }

                    SendImageResponse(writer, pngData);
					Logger.LogInfo($"Poslat PNG odgovor za fajl: {requestedPath}");

				}
                catch (Exception ex)
                {
                    Logger.LogError("Greška pri obradi zahteva: " + ex.Message);
                }
            }
        }

        private static void SendImageResponse(StreamWriter writer, byte[] imageData)
        {
            writer.WriteLine("HTTP/1.1 200 OK");
            writer.WriteLine("Content-Type: image/png");
            writer.WriteLine("Content-Length: " + imageData.Length);
            writer.WriteLine(); // kraj headera
            writer.BaseStream.Write(imageData, 0, imageData.Length);
			//Logger.LogInfo($"Poslat PNG odgovor za fajl: {requestedPath}"); moze i ovde da se loguje u zavisnoti od toga kako se odluci
		}

        private static void SendNotFound(StreamWriter writer)
        {
            string body = "Nije nadjena slika";
            writer.WriteLine("HTTP/1.1 404 Not Found");
            writer.WriteLine("Content-Type: text/plain; charset=utf-8");
            writer.WriteLine("Content-Length: " + Encoding.UTF8.GetByteCount(body));
            writer.WriteLine();
            writer.WriteLine(body);
        }

        private static void SendBadRequest(StreamWriter writer)
        {
            string body = "Nije nam stigao GET zahtev sa 3 parametra. Primer pravilnog zahteva:  GET /test.jpg HTTP/1.1";
            writer.WriteLine("HTTP/1.1 400 Bad Request");
            writer.WriteLine("Content-Type: text/plain; charset=utf-8");
            writer.WriteLine("Content-Length: " + Encoding.UTF8.GetByteCount(body));
            writer.WriteLine();
            writer.WriteLine(body);
        }
    }
}
