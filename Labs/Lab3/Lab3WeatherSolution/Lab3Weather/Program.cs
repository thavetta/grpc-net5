using Lab3Weather;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

//Log.Logger = new LoggerConfiguration().ReadFrom
//                .Configuration().CreateLogger();

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog((context,services,configuration) => configuration.ReadFrom.Configuration(context.Configuration));
builder.Services.AddScoped<IRandomInt, RandomIntLite>();
builder.Services.AddGrpc().AddServiceOptions<Lab3Weather.Services.AirportWeatherService>(options =>
{
    options.MaxReceiveMessageSize = 1024;
    options.MaxSendMessageSize = 1024; //nastavení velikosti na 100 skonèí výjimkou
    options.EnableDetailedErrors = true;
});
var app = builder.Build();

app.MapGrpcService<Lab3Weather.Services.AirportWeatherService>();
app.MapGet("/",() => "Bez klienta gRPC smula...");

app.Run();
