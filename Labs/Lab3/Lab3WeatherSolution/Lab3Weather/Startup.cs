//using Lab3Weather.Services;
//using Microsoft.AspNetCore.Builder;
//using Microsoft.AspNetCore.Hosting;
//using Microsoft.AspNetCore.Http;
//using Microsoft.Extensions.DependencyInjection;
//using Microsoft.Extensions.Hosting;
//using Microsoft.Extensions.Logging;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading.Tasks;
//using Serilog.AspNetCore;
//using Microsoft.Extensions.Configuration;
//using Serilog;

//namespace Lab3Weather
//{
//    public class Startup
//    {
//        public Startup(IConfiguration configuration)
//        {
//            Log.Logger = new LoggerConfiguration().ReadFrom
//                .Configuration(configuration).CreateLogger();
//        }

//        // This method gets called by the runtime. Use this method to add services to the container.
//        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
//        public void ConfigureServices(IServiceCollection services)
//        {
//            services.AddScoped<IRandomInt, RandomIntLite>();
//            services.AddGrpc( options =>
//            {
//                options.MaxReceiveMessageSize = 1024;
//                options.MaxSendMessageSize = 1024; //nastavení velikosti na 100 skončí výjimkou
//                options.EnableDetailedErrors = true;
//            });

//            //Odpoznámkováním tohoto bloku se stane service Singletonem
//            //var sp = services.BuildServiceProvider();
//            //var service = new AirportWeatherService(sp.GetService<ILogger<AirportWeatherService>>());
//            //services.AddSingleton(service);
//        }

//        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
//        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
//        {
//            if (env.IsDevelopment())
//            {
//                app.UseDeveloperExceptionPage();
//            }

//            app.UseRouting();

//            app.UseEndpoints(endpoints =>
//            {
//                endpoints.MapGrpcService<Services.AirportWeatherService>();

//                endpoints.MapGet("/", async context =>
//                {
//                    await context.Response.WriteAsync("Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");
//                });
//            });
//        }
//    }
//}
