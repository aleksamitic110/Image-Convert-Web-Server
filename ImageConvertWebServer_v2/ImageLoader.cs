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
		public static async Task<byte[]> ConvertJpgToPngAsync(string jpgFilePath)
		{
			
			if (!File.Exists(jpgFilePath))
			{
				await Logger.LogErrorAsync($"Fajl za konverziju ne postoji: {jpgFilePath}");
				return null;
			}

			try
			{
				// Asinhrono citanje fajla, ne blokira se nit
				byte[] jpgBytes; //byte[] jpgBytes = await File.ReadAllBytesAsync(jpgFilePath); ne radi u ovoj verziji pa moramo rucno
				using (var fs = new FileStream(jpgFilePath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, useAsync: true))
				{
					jpgBytes = new byte[fs.Length];
					await fs.ReadAsync(jpgBytes, 0, (int)fs.Length);
				}

				using (var jpgStream = new MemoryStream(jpgBytes))
				using (var jpgImage = Image.FromStream(jpgStream))
				using (var pngStream = new MemoryStream())
				{
					jpgImage.Save(pngStream, ImageFormat.Png);
					byte[] pngBytes = pngStream.ToArray();
					await Logger.LogInfoAsync($"Fajl {Path.GetFileName(jpgFilePath)} konvertovan u PNG");
					return pngBytes;
				}
			}
			catch (Exception ex)
			{
				await Logger.LogErrorAsync($"Greška prilikom konverzije fajla {Path.GetFileName(jpgFilePath)}: {ex.Message}");
				return null;
			}
		}
	}
}
