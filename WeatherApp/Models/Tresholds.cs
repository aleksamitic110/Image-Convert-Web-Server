using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WeatherApp.Models
{
	internal class Thresholds //Neki prag za vrednosti moze da bude koji god kako bi imali kad da odradimo alert
	{
		public double PM10_MAX {get; set;}
		public double PM25_MAX { get; set; }
		public double CO_MAX { get; set; }
		public double NO2_MAX { get; set; }


		public Thresholds(double PM10_MAX, double PM25_MAX, double CO_MAX, double NO2_MAX) {
			this.PM25_MAX = PM25_MAX;
			this.PM10_MAX = PM10_MAX;
			this.CO_MAX = CO_MAX;
			this.NO2_MAX = NO2_MAX;
		}
	}
}
