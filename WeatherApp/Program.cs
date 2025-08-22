using System;
using System.Reactive.Linq;
using WeatherApp.Server;
using WeatherApp.Models;
using System.Reactive.Concurrency;

// Kreiramo server
var server = new ReactiveServer("http://localhost:5050/");

// Pretplata na Rx tok da vidimo svaki zahtev u konzoli
server.RequestStream
	  .ObserveOn(TaskPoolScheduler.Default)
	  .Subscribe(log =>
	  {
		  Console.WriteLine($"[RX STREAM] {log.Timestamp} {log.Method} {log.RequestUrl} Success: {log.Success}");
	  });

// Start servera (asinhrono)
await server.StartAsync();

