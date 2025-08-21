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
			// File.Exists je i dalje sinhrone, ali je brza operacija.
			if (!File.Exists(jpgFilePath))
			{
				await Logger.LogErrorAsync($"Fajl za konverziju ne postoji: {jpgFilePath}");
				return null;
			}

			try
			{
				// 1. Asinhrono čitanje fajla sa diska da ne blokiramo nit.
				//byte[] jpgBytes = await File.ReadAllBytesAsync(jpgFilePath); ne radi u ovoj verziji pa moramo rucno

				byte[] jpgBytes;
				using (var fs = new FileStream(jpgFilePath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, useAsync: true))
				{
					jpgBytes = new byte[fs.Length];
					await fs.ReadAsync(jpgBytes, 0, (int)fs.Length);
				}

				// 2. Ostatak konverzije je CPU-bound (radi u memoriji) i ostaje sinhron.
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
