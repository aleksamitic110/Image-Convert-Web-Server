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
        // Keš: kljuc = ime jpg fajla, vrednost = PNG bajt niz, ConcurrentDictionary zato sto je thread-safe u odnosu na obican
        private static ConcurrentDictionary<string, byte[]> _cache = new ConcurrentDictionary<string, byte[]>();

        public static byte[] GetPngImage(string jpgFilePath)
        {
            string key = Path.GetFileName(jpgFilePath).ToLowerInvariant(); // GetFileName() vraca samo naziv file-a

            // Ako imamo keširani PNG - vratimo ga
            if (_cache.TryGetValue(key, out byte[] cachedData))
            {
                Logger.LogInfo($"Keširan PNG vraćen za: {key}");
                return cachedData;
            }
            else
            {
				Logger.LogInfo($"Nije pronadjen PNG u kešu za: {key}");

				byte[] pngBytes = ImageLoader.ConvertJpgToPng(jpgFilePath);

                if (pngBytes != null)
                {
                    _cache[key] = pngBytes;
                    Logger.LogInfo($"Dodat PNG u keš za: {key}");
                }
				return pngBytes;
            }
        }
    }
}
