using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace ImageConvertWebServer
{
	public static class ImageLoader
	{
		public static byte[] ConvertJpgToPng(string jpgFilePath)
		{
			if (!File.Exists(jpgFilePath))
			{
				Logger.LogError($"Fajl za konverziju ne postoji: {jpgFilePath}");
				return null;
			}

			try
			{
				using (var jpgImage = Image.FromFile(jpgFilePath))
				using (var ms = new MemoryStream())
				{
					// Sačuvaj u PNG format u memorijski stream
					jpgImage.Save(ms, ImageFormat.Png);
					byte[] pngBytes = ms.ToArray();
					Logger.LogInfo($"Fajl {Path.GetFileName(jpgFilePath)} konvertovan PNG");
					return pngBytes;
				}
			}
			catch (Exception ex)
			{
				Logger.LogError($"Greška prilikom konverzije fajla {Path.GetFileName(jpgFilePath)}: {ex.Message}");
				return null;
			}
		}
	}
}
