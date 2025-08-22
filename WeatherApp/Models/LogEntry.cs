using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WeatherApp.Models
{
	public class LogEntry
	{
		public DateTime Timestamp { get; set; }

		public string RequestUrl { get; set; } = "";
		public string Method { get; set; } = "";
		public string ClientIP { get; set; } = "";

		public bool Success { get; set; }
		public string? ErrorMessage { get; set; }

		// Novo: status HTTP odgovora i trajanje obrade (ms)
		public int? StatusCode { get; set; }
		public long? ElapsedMs { get; set; }

		// Novo: koordinate iz upita, radi filtriranja po lokaciji
		public double? Latitude { get; set; }
		public double? Longitude { get; set; }

		// Novo: poslednji dobijeni AQ rezultat (radi agregacije)
		public AirQualityData? AirQuality { get; set; }
	}
}
