using Lab2Weather;
using Lab2Weather.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddGrpc().AddServiceOptions<AirportWeatherService>(options =>
{
    options.MaxSendMessageSize = 2 * 1024 * 1024;
    options.MaxReceiveMessageSize = 5 * 1024 * 1024;
});

var app = builder.Build();
string error = "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909";

app.MapGrpcService<AirportWeatherService>();
app.MapGet("/", async context => { await context.Response.WriteAsync(error); });

app.Run();


