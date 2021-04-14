using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;

namespace SignalrMessagepackExampleConsole
{
    class Program
    {
        static async Task Main(string[] args)
        {
            const bool isUsingMessagepack = false;
            const int numberOfPackets = 2000;
            const int packetSizeInBytes = 256 * 1024;

            Console.WriteLine($"isUsingMessagepack={isUsingMessagepack} | numberOfPackets={numberOfPackets} | packetSize={packetSizeInBytes} Bytes");

            var hubConnectionBuilder = new HubConnectionBuilder()
                .WithUrl("http://localhost:5000/data");

            if (isUsingMessagepack)
                hubConnectionBuilder.AddMessagePackProtocol();
            else
                hubConnectionBuilder.AddJsonProtocol();
                
            var hubConnection = hubConnectionBuilder.Build();

            await hubConnection.StartAsync();
            Console.WriteLine("Connected to hub");

            var cancellationTokenSource = new CancellationTokenSource();
            var stream = hubConnection.StreamAsync<object>("SendData", numberOfPackets, packetSizeInBytes, cancellationTokenSource.Token);
            var startDate = DateTime.Now;
            var receivedBytes = 0;
            var packetNumber = 0;

            await foreach (var data in stream.WithCancellation(cancellationTokenSource.Token))
            {
                packetNumber++;
                receivedBytes += isUsingMessagepack ? ((byte[])data).Length : Convert.FromBase64String(data.ToString()).Length;
                var diffTimeInSeconds = GetDiffTimeInSeconds(startDate);

                if (packetNumber % 100 == 0)
                    Console.WriteLine($"Received data ({packetNumber * 100 / numberOfPackets}%). {packetNumber / diffTimeInSeconds} packets/s | Bandwidth={(receivedBytes / 1024) / (diffTimeInSeconds)} Kbytes/s");
            }

            Console.WriteLine($"Streaming completed. It took {GetDiffTimeInSeconds(startDate)}s");
        }

        private static double GetDiffTimeInSeconds(DateTime startTime)
        {
            return (DateTime.Now - startTime).TotalMilliseconds / 1000;
        }
    }
}
