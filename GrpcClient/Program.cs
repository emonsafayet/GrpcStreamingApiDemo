﻿using Grpc.Net.Client;
using GrpcService;

namespace GrpcClient
{
    public class Program
    {
        private static Random random;
        public static async Task Main(string[] args)
        {

            random = new Random();

            //await ServerStreamingDemo();
            //await ClientStreamingDemo();
            await BidirectionalStreamingDemo();
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }

        private static async Task BidirectionalStreamingDemo()
        {
            var channel = GrpcChannel.ForAddress("http://localhost:5092");
            var client = new StreamDemo.StreamDemoClient(channel);
            var stream = client.BidirectionalStreamingDemo();

            var requestTask = Task.Run(async () =>
            {
                for (int i = 0; i<= 10; i++)
                {
                    var randomNumber = random.Next(1, 10);
                    await Task.Delay(1000);
                    await stream.RequestStream.WriteAsync(new Test { TestMessage=i.ToString() });
                    Console.WriteLine("Send Client Request: " + i);

                }
                await stream.RequestStream.CompleteAsync();
            });
            var responseTask = Task.Run(async () => {

                while (await stream.ResponseStream.MoveNext(CancellationToken.None))
                {
                    Console.WriteLine("Receive Client Response: " +stream.ResponseStream.Current.TestMessage);
                }
                Console.WriteLine("**** Completed Client Streaming ***** ");
            });

            await Task.WhenAll(requestTask, responseTask);
            await channel.ShutdownAsync();
        }
        private static async Task ClientStreamingDemo()
        {
           
            var channel = GrpcChannel.ForAddress("http://localhost:5092");
            var client = new StreamDemo.StreamDemoClient(channel);
            var stream = client.ClientStreamingDemo();
            for (int i = 1; i < 10; i++)
            {
               await stream.RequestStream.WriteAsync(new Test { TestMessage = $"Message {i}" });
                var randomNumber = random.Next(1, 10);
                await Task.Delay( 1000);

            }

            await stream.RequestStream.CompleteAsync();
            await channel.ShutdownAsync();
            Console.WriteLine("Completed Client Streaming");
        }

        private static async Task ServerStreamingDemo()
        {
            var channel = GrpcChannel.ForAddress("http://localhost:5092");
            var client = new StreamDemo.StreamDemoClient(channel);
            var response = client.ServerStreamingDemo(new Test { TestMessage="Test" });
            while (await response.ResponseStream.MoveNext(CancellationToken.None))
            {
                var value = response.ResponseStream.Current.TestMessage;
                Console.WriteLine(value);
            }               

            Console.WriteLine("Server Streaming Completed");
            await channel.ShutdownAsync();
        }
    }
}