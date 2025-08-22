using System;
using System.Reactive.Linq;
using WeatherApp.Server;
using WeatherApp.Models;
using System.Reactive.Concurrency;
using WeatherApp.Services;

var server = new ReactiveServer("http://localhost:5050/");
var meteoService = new OpenMeteoService();

ReactiveProcessing.SetupPipeline(server, meteoService);

Console.WriteLine("Starting server...");
await server.StartAsync();



