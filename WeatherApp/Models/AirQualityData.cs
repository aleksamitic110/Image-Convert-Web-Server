using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WeatherApp.Models
{
	public class AirQualityData
	{
		public DateTime Timestamp { get; set; }
		public double PM10 { get; set; }
		public double PM25 { get; set; }
		public double CO { get; set; }
		public double NO2 { get; set; }
	}
}
