using Lab3Weather;
using Serilog;

//Log.Logger = new LoggerConfiguration().ReadFrom
//                .Configuration().CreateLogger();

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog((context,services,configuration) => configuration.ReadFrom.Configuration(context.Configuration));
builder.Services.AddScoped<IRandomInt, RandomIntLite>();
builder.Services.AddGrpc().AddServiceOptions<Lab3Weather.Services.AirportWeatherService>(options =>
{
    options.MaxReceiveMessageSize = 1024;
    options.MaxSendMessageSize = 1024; //nastaven� velikosti na 100 skon�� v�jimkou
    options.EnableDetailedErrors = true;
});
var app = builder.Build();

app.MapGrpcService<Lab3Weather.Services.AirportWeatherService>();
app.MapGet("/",() => "Bez klienta gRPC smula...");

app.Run();
