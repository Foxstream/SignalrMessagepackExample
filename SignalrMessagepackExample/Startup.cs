using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.SignalR;

namespace SignalrMessagepackExample
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>().UseHttpSys(options =>
                    {
                        options.AllowSynchronousIO = true;
                        options.MaxConnections = -1;
                        options.MaxRequestBodySize = null;
                        options.UrlPrefixes.Add("http://*:5000");
                    });
                });
    }

    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services
                .AddSignalR()
                .AddMessagePackProtocol();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHub<DataHub>("/data", options =>
                {
                    options.Transports = HttpTransportType.WebSockets;
                });
            });
        }
    }

    public class DataHub : Hub
    {
        public async IAsyncEnumerable<byte[]> SendData(
            int count,
            int size,
            [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            var random = new Random();
            var bytes = new byte[size];
            for (var i = 0; i < count; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                random.NextBytes(bytes);
                yield return bytes;

                await Task.CompletedTask;
            }
        }
    }
}
