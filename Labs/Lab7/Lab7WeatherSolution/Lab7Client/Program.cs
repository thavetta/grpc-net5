using System;
using System.Net.Http;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Grpc.Core;
using Grpc.Core.Interceptors;
using Grpc.Net.Client;
using Grpc.Net.Client.Configuration;

namespace Lab7Client
{
    class Program
    {
        private const string Address = "https://localhost:5001";
        static async Task Main(string[] args)
        {
            Console.WriteLine("Ready for test");
            Console.ReadKey();

            string token = await Authenticate();

            try
            { 
                var reply = await CallClient("BRQ", token);
                
                OutputResult(reply);

                reply = await CallClient("BRQ",token, true);

                OutputResult(reply);
                Console.WriteLine("Predpoved:");
                OutputResult(reply, 1);
                OutputResult(reply, 2);
                OutputResult(reply, 3);
                Console.WriteLine("**********************");
            }
            catch (RpcException ex)
            {
                Console.WriteLine("Chyba: " + ex.Status.Detail);
            }

            await ServerStream();

            await Sumuj();
            

            _channel.Dispose();

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }

        private async static Task ServerStream()
        {
            var cts = new CancellationTokenSource();
            cts.CancelAfter(TimeSpan.FromSeconds(60));

            using var call = _client.GetWeatherStream(new AirportRequest() { AirportCode = "BTS" },
                cancellationToken: cts.Token);

            try
            {
                await foreach(var message in call.ResponseStream.ReadAllAsync())
                {
                    Console.WriteLine("Info v case: " + DateTime.Now.TimeOfDay);
                    Console.WriteLine("Teplota " + message.Temperature);
                }
            }
            catch (RpcException ex) when (ex.StatusCode == StatusCode.Cancelled)
            {
                Console.WriteLine("Stream ukoncen");
            }
        }

        private static async Task Sumuj()
        {
            Random random = new Random();
            using var call = _client.Sum();
            for (int i = 0; i < 10; i++)
            {
                int number = random.Next(100);
                Console.WriteLine("Posilam " + number);
                await call.RequestStream.WriteAsync(new NumberRequest() { Number = number });
                await Task.Delay(1500);

            }
            await call.RequestStream.CompleteAsync();

            var response = await call;
            Console.WriteLine("Soucet je " + response.Number);

        }

        private static GrpcChannel _channel;
        private static CallInvoker _invoker;
        private static AirportWeather.AirportWeatherClient _client;

        private static AsyncUnaryCall<WeatherReply> CallClient(string code, string? token = null, bool useOld = false)
        {
            if (!(useOld && _channel != null))
            {
                _channel = CreateChannel();
                _invoker = _channel.Intercept(new ClientLoggerInterceptor());
            }

            if (!(useOld && _client != null))
            {
                _client = new AirportWeather.AirportWeatherClient(_invoker);
            }
            Metadata? headers = null;
            if (token != null)
            {
                headers = new Metadata();
                headers.Add("Authorization", $"Bearer {token}");
            }
            var reply = _client.GetWeatherAsync(
                new AirportRequest { AirportCode = code }, headers);
                return reply;
            
        }

        private static GrpcChannel CreateChannel()
        {
            var methodConfig = new MethodConfig
            {
                Names = { MethodName.Default },
                RetryPolicy = new RetryPolicy
                {
                    MaxAttempts = 10,
                    InitialBackoff = TimeSpan.FromSeconds(0.5),
                    MaxBackoff = TimeSpan.FromSeconds(5),
                    BackoffMultiplier = 1.5,
                    RetryableStatusCodes = { StatusCode.Unavailable }
                }
            };

            return GrpcChannel.ForAddress(Address, new GrpcChannelOptions
            {
                ServiceConfig = new ServiceConfig { MethodConfigs = { methodConfig } }
            });
        }

        private static async Task<string> GetRetryCount(Task<Metadata> responseHeadersTask)
        {
            var headers = await responseHeadersTask;
            var previousAttemptCount = headers.GetValue("grpc-previous-rpc-attempts");
            return previousAttemptCount != null ? $"(retry count: {previousAttemptCount})" : string.Empty;
        }

        private static void OutputResult(WeatherReply reply, int index = 0)
        {
            WeatherInfo weather = index == 0 ? reply.Actual : reply.Forecast[index];
            Console.WriteLine("Vysledek: ");
            Console.WriteLine("Teplota: " + weather.Temperature);
            Console.WriteLine("Tlak: " + weather.Pressure);

            switch (weather.WindDirectionCase)
            {
                case WeatherInfo.WindDirectionOneofCase.Code:
                    Console.WriteLine("Smer vetru: " + weather.Code);
                    break;
                case WeatherInfo.WindDirectionOneofCase.Degree:
                    Console.WriteLine("Smer vetru: " + weather.Degree);
                    break;
            }

            Console.WriteLine("Status: " + weather.Status);
            Console.WriteLine("Varovani:");
            foreach (var item in weather.Warnning)
            {
                Console.WriteLine("- " + item);
            }
        }

        static async Task<string> Authenticate()
        {
            Console.WriteLine($"Authenticating as {Environment.UserName}...");
            using var httpClient = new HttpClient();
            using var request = new HttpRequestMessage
            {
                RequestUri = new Uri($"{Address}/generateJwtToken?name={HttpUtility.UrlEncode(Environment.UserName)}"),
                Method = HttpMethod.Get,
                Version = new Version(2, 0)
            };
            using var tokenResponse = await httpClient.SendAsync(request);
            tokenResponse.EnsureSuccessStatusCode();

            var token = await tokenResponse.Content.ReadAsStringAsync();
            Console.WriteLine("Successfully authenticated.");

            return token;
        }

    }

}
