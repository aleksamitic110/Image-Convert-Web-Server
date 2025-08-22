using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using WeatherApp.Models;
using WeatherApp.Services;

namespace WeatherApp.Server
{
	public static class Thresholds
	{
		public const double PM10_MAX = 50;
		public const double PM25_MAX = 25;
		public const double CO_MAX = 10;
		public const double NO2_MAX = 40;
	}



	public static class ReactiveProcessing
	{
		public static void SetupPipeline(ReactiveServer server, OpenMeteoService meteoService)
		{
			// Glavni pipeline za obradu HTTP zahteva
			var pipeline = server.RequestStream
				.ObserveOn(TaskPoolScheduler.Default) // paralelna obrada
				.SelectMany(ctx => Observable.FromAsync(() => HandleRequest(ctx, meteoService))
											 .Retry(3)
											 .Catch<LogEntry, Exception>(ex =>
											 {
												 Console.WriteLine($"[PIPELINE ERROR] {ex.Message}");
												 return Observable.Empty<LogEntry>();
											 }))
				.Publish()
				.RefCount();

			// 1️⃣ Svi uspešni zahtevi
			pipeline.Where(log => log.Success)
					.Subscribe(log =>
					{
						Console.WriteLine($"[SUCCESS] {log.Method} {log.RequestUrl} from {log.ClientIP}");
					});

			// 2️⃣ Alert za visok CO (> 100 µg/m3)
			pipeline.Where(log => log.AirQuality?.CO > 100)
					.Subscribe(log =>
					{
						Console.WriteLine($"⚠️ HIGH CO ALERT! {log.AirQuality.CO} µg/m3 at {log.Timestamp}");
					});

			// 3️⃣ Alert za visok NO2 (> 200 µg/m3)
			pipeline.Where(log => log.AirQuality?.NO2 > 200)
					.Subscribe(log =>
					{
						Console.WriteLine($"⚠️ HIGH NO2 ALERT! {log.AirQuality.NO2} µg/m3 at {log.Timestamp}");
					});

			// 4️⃣ Agregacija – prosečne vrednosti po 5 minuta
			pipeline.Where(log => log.AirQuality != null)
					.Buffer(TimeSpan.FromMinutes(5))
					.Where(buf => buf.Count > 0)
					.Subscribe(buf =>
					{
						var avgPM10 = buf.Average(l => l.AirQuality.PM10);
						var avgPM25 = buf.Average(l => l.AirQuality.PM25);
						var avgCO = buf.Average(l => l.AirQuality.CO);
						var avgNO2 = buf.Average(l => l.AirQuality.NO2);

						Console.WriteLine($"[AGGREGATE 5min] Avg PM10={avgPM10:F2}, PM2.5={avgPM25:F2}, CO={avgCO:F2}, NO2={avgNO2:F2}");
					});
		}

		private static async Task<LogEntry> HandleRequest(HttpListenerContext context, OpenMeteoService meteoService)
		{
			var sw = System.Diagnostics.Stopwatch.StartNew();

			string url = context.Request.RawUrl ?? "/";
			string method = context.Request.HttpMethod;

			string clientIP = context.Request.RemoteEndPoint?.Address.ToString() ?? "unknown";
			if (clientIP == "::1") clientIP = "127.0.0.1";

			var logEntry = new LogEntry
			{
				Timestamp = DateTime.Now,
				RequestUrl = url,
				Method = method,
				ClientIP = clientIP,
				Success = false
			};

			try
			{
				if (url == "/favicon.ico")
				{
					context.Response.StatusCode = 204;
					context.Response.Close();
					logEntry.Success = true;
					logEntry.StatusCode = 204;
					return logEntry;
				}

				// Parsiranje query parametara
				double latitude = 0, longitude = 0;
				var queryParams = System.Web.HttpUtility.ParseQueryString(context.Request.Url.Query);
				double.TryParse(queryParams["lat"], out latitude);
				double.TryParse(queryParams["lon"], out longitude);

				logEntry.Latitude = latitude;
				logEntry.Longitude = longitude;

				// Poziv OpenMeteo API-ja
				AirQualityData? airQuality = await meteoService.GetAirQualityAsync(latitude, longitude);
				logEntry.AirQuality = airQuality;

				string responseString;
				if (airQuality != null)
				{
					responseString = System.Text.Json.JsonSerializer.Serialize(airQuality);
					context.Response.ContentType = "application/json";
					logEntry.Success = true;
					logEntry.StatusCode = 200;
				}
				else
				{
					responseString = "{\"error\":\"Unable to fetch air quality\"}";
					context.Response.ContentType = "application/json";
					logEntry.Success = false;
					logEntry.StatusCode = 502;
				}

				byte[] buffer = Encoding.UTF8.GetBytes(responseString);
				context.Response.ContentLength64 = buffer.Length;
				await context.Response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
				context.Response.OutputStream.Close();

				await Logger.LogRequestAsync($"{method} {url} from {clientIP}");
			}
			catch (Exception ex)
			{
				logEntry.Success = false;
				logEntry.ErrorMessage = ex.Message;
				logEntry.StatusCode = 500;
				await Logger.LogErrorAsync($"Error handling request: {ex.Message}");
			}
			finally
			{
				sw.Stop();
				logEntry.ElapsedMs = sw.ElapsedMilliseconds;
			}

			return logEntry;
		}
	}

























	//---------------------------------------------------------------- LOS NACIN

	/*
	public static class ReactiveProcessing
	{
		public static void SetupSubscriptions(ReactiveServer server)
		{
			// Primer 1: Svi uspešni zahtevi
			server.RequestStream
				.Where(log => log.Success && log.AirQuality != null)
				.Select(log => log.AirQuality!) // ovde ! znači "sigurno nije null"
				.Retry(3) // pokušaj 3 puta ako dođe do greške
				.Catch<AirQualityData, Exception>(ex =>
				{
					Console.WriteLine($"[ERROR] Stream failed: {ex.Message}");
					return Observable.Empty<AirQualityData>(); // fallback: prazan stream
				})
				.Subscribe(aq =>
				{
					Console.WriteLine($"Retry stream uspešno obradio podatke: PM10={aq.PM10}, PM2.5={aq.PM25}, CO={aq.CO}, NO2={aq.NO2}");
				});


			// Primer 2: Samo oni gde je dobijen CO podatak
			server.RequestStream
				.Where(log => log.AirQuality != null && log.AirQuality.CO > 0)
				.Subscribe(log =>
				{
					Console.WriteLine($"CO data = {log.AirQuality.CO} µg/m³ at {log.Timestamp} from {log.ClientIP}");
				});

			// Primer 3: Samo oni gde je NO2 visok (npr. iznad 40)
			server.RequestStream
				.Where(log => log.AirQuality != null && log.AirQuality.NO2 > 40)
				.Subscribe(log =>
				{
					Console.WriteLine($"ALERT: High NO₂ = {log.AirQuality.NO2} µg/m³ at {log.Timestamp}");
				});

			// Primer 4: Agregacija na svakih 5 minuta
			server.RequestStream
				.Where(log => log.AirQuality != null)
				.Buffer(TimeSpan.FromMinutes(5)) // sakuplja logove tokom 5 minuta
				.Where(buffer => buffer.Any())   // ignoriši prazne
				.Select(buffer => new
				{
					AvgPM10 = buffer.Average(l => l.AirQuality.PM10),
					AvgPM25 = buffer.Average(l => l.AirQuality.PM25),
					AvgCO = buffer.Average(l => l.AirQuality.CO),
					AvgNO2 = buffer.Average(l => l.AirQuality.NO2),
					Count = buffer.Count
				})
				.Subscribe(avg =>
				{
					Console.WriteLine($"5-min avg from {avg.Count} requests -> PM10={avg.AvgPM10:F2}, PM2.5={avg.AvgPM25:F2}, CO={avg.AvgCO:F2}, NO2={avg.AvgNO2:F2}");
				});



			server.RequestStream
				.Where(log => log.Success && log.AirQuality != null)
				.Subscribe(log =>
				{
					var aq = log.AirQuality!;

					if (aq.PM10 > Thresholds.PM10_MAX)
						Console.WriteLine($"ALERT PM10 = {aq.PM10} µg/m3 at {log.Timestamp}");
					if (aq.PM25 > Thresholds.PM25_MAX)
						Console.WriteLine($"ALERT PM2.5 = {aq.PM25} µg/m3 at {log.Timestamp}");
					if (aq.CO > Thresholds.CO_MAX)
						Console.WriteLine($"ALERT CO = {aq.CO} µg/m3 at {log.Timestamp}");
					if (aq.NO2 > Thresholds.NO2_MAX)
						Console.WriteLine($"ALERT NO2 = {aq.NO2} µg/m3 at {log.Timestamp}");
				});
		}
	}
	*/
}
