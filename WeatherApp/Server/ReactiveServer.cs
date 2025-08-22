using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Diagnostics;
using WeatherApp.Models;
using WeatherApp.Services;

namespace WeatherApp.Server
{


	public class ReactiveServer
	{
		private readonly HttpListener _listener;
		private readonly Subject<HttpListenerContext> _requestStream;

		public ReactiveServer(string prefix)
		{
			_listener = new HttpListener();
			_listener.Prefixes.Add(prefix);
			_requestStream = new Subject<HttpListenerContext>();
		}

		public IObservable<HttpListenerContext> RequestStream => _requestStream;

		public async Task StartAsync()
		{
			_listener.Start();
			Console.WriteLine("Server started... Listening for requests.");

			while (true)
			{
				var context = await _listener.GetContextAsync();
				_requestStream.OnNext(context); // Push u Rx pipeline
			}
		}
	}



	/*
	public class ReactiveServer
	{
		private readonly HttpListener _listener;
		private readonly Subject<LogEntry> _requestStream;

		public ReactiveServer(string prefix)
		{
			_listener = new HttpListener();
			_listener.Prefixes.Add(prefix);
			_requestStream = new Subject<LogEntry>();
		}

		// Observable koji klijenti mogu da se pretplate
		public IObservable<LogEntry> RequestStream => _requestStream;

		public async Task StartAsync()
		{
			_listener.Start();
			await Logger.LogInfoAsync("Server started... Listening for requests.");

			while (true)
			{
				var context = await _listener.GetContextAsync();
				_ = HandleRequestAsync(context); // fire-and-forget
			}
		}

		private async Task HandleRequestAsync(HttpListenerContext context)
		{

			// Ignorišemo favicon zahtev
			if (context.Request.RawUrl == "/favicon.ico")
			{
				context.Response.StatusCode = 204; // No Content
				context.Response.Close();
				return; // izlazimo odmah
			}

			var sw = Stopwatch.StartNew();

			string url = context.Request.RawUrl ?? "/";
			string method = context.Request.HttpMethod;

			string clientIP = context.Request.RemoteEndPoint.Address.ToString(); 
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
				double latitude = 0;
				double longitude = 0;
				var query = context.Request.Url.Query;
				var queryParams = HttpUtility.ParseQueryString(query);

				double.TryParse(queryParams["lat"], out latitude);
				double.TryParse(queryParams["lon"], out longitude);

				logEntry.Latitude = latitude;
				logEntry.Longitude = longitude;



				var openMeteoService = new OpenMeteoService();
				AirQualityData? airQuality = await openMeteoService.GetAirQualityAsync(latitude, longitude);

				string responseString;
				if (airQuality != null)
				{
					responseString = System.Text.Json.JsonSerializer.Serialize(airQuality);
					context.Response.ContentType = "application/json";

					logEntry.Success = true;
					logEntry.StatusCode = 200;
					logEntry.AirQuality = airQuality;

				}
				else
				{
					responseString = "{\"error\":\"Unable to fetch air quality\"}";
					context.Response.ContentType = "application/json";

					logEntry.Success = false;
					logEntry.StatusCode = 502; // ili 500, kako želiš

				}

				byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
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
				// Emitujemo logEntry u Rx stream
				sw.Stop();
				logEntry.ElapsedMs = sw.ElapsedMilliseconds;
				_requestStream.OnNext(logEntry);
			}
		}
	}
	*/
}
