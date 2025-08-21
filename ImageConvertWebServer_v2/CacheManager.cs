using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;

namespace ImageConvertWebServer
{
	internal class CacheManager
	{
		private static ConcurrentDictionary<string, byte[]> _cache = new ConcurrentDictionary<string, byte[]>();

		
		public static async Task<byte[]> GetPngImageAsync(string jpgFilePath)
		{
			string key = Path.GetFileName(jpgFilePath).ToLowerInvariant();

			if (_cache.TryGetValue(key, out byte[] cachedData))
			{
				await Logger.LogInfoAsync($"Keširan PNG vraćen za: {key}");
				return cachedData;
			}
			else
			{
				await Logger.LogInfoAsync($"Nije pronadjen PNG u kešu za: {key}");

				
				byte[] pngBytes = await ImageLoader.ConvertJpgToPngAsync(jpgFilePath);

				if (pngBytes != null)
				{
					_cache[key] = pngBytes;
					await Logger.LogInfoAsync($"Dodat PNG u keš za: {key}");
				}
				return pngBytes;
			}
		}
	}
}
