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
		public int? StatusCode { get; set; }
		public long? ElapsedMs { get; set; }

		public double? Latitude { get; set; }
		public double? Longitude { get; set; }

		public AirQualityData? AirQuality { get; set; }
	}
}
