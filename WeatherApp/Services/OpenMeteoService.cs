using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web;
using WeatherApp.Models;

namespace WeatherApp.Services
{
	public class OpenMeteoService
	{
		private readonly HttpClient _httpClient;

		public OpenMeteoService()
		{
			_httpClient = new HttpClient();
		}

		public async Task<AirQualityData?> GetAirQualityAsync(double latitude, double longitude)
		{
			try
			{
				string url = $"https://air-quality-api.open-meteo.com/v1/air-quality?latitude={latitude}&longitude={longitude}&hourly=pm10,pm2_5,carbon_monoxide,nitrogen_dioxide";

				var response = await _httpClient.GetAsync(url);
				response.EnsureSuccessStatusCode();
				var jsonString = await response.Content.ReadAsStringAsync();

				// Parsiranje minimalnog JSON-a
				using JsonDocument doc = JsonDocument.Parse(jsonString);
				var root = doc.RootElement;

				// Ovde ćemo samo uzeti prvi vremenski slot za primer
				var hourly = root.GetProperty("hourly");
				var time = hourly.GetProperty("time")[0].GetDateTime();
				var pm10 = hourly.GetProperty("pm10")[0].GetDouble();
				var pm25 = hourly.GetProperty("pm2_5")[0].GetDouble();
				var co = hourly.GetProperty("carbon_monoxide")[0].GetDouble();
				var no2 = hourly.GetProperty("nitrogen_dioxide")[0].GetDouble();

				return new AirQualityData
				{
					Timestamp = time,
					PM10 = pm10,
					PM25 = pm25,
					CO = co,
					NO2 = no2
				};
			}
			catch (Exception ex)
			{
				await Logger.LogErrorAsync($"Error fetching air quality: {ex.Message}");
				return null;
			}
		}
	}
}
