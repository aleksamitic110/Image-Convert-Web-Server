using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageConvertWebServer
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.Title = "Image Converter Web Server";

            //Definisemo port na kom ce server osluskivati i rootFolder za koj cemo proveravati slike - 
            int port = 5050;
            
			string projectRoot = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\.."));
			string rootFolder = Path.Combine(projectRoot, "Images");

            //Pravimo server i pokrecemo ga
			var server = new ImageServer(port, rootFolder);
            server.Start();

            //Prikazivnaje loga da je server pokrenut, na kom portu i koji mu je root folder zadat
            Console.WriteLine("http://localhost:" + port);
            Console.WriteLine("Root folder with images: " + rootFolder);

            //Server radi sve dok ne kliknes Enter
            Console.ReadLine();

            server.Stop();
        }
    }
}
