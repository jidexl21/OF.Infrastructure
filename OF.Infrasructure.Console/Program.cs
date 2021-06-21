using OF.Infrastructure.Messaging;
using System;

namespace OF.Infrasructure.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            Environment.SetEnvironmentVariable("ampqhost", "");
            var x = new Publisher();
            var message = new QueueMessage {
                Topic = "sample-topic",
                Message =$"Hello World {Guid.NewGuid().ToString("N")}"
            }; 

            var task = x.PublishAsync(message);
            task.Wait();
            System.Console.WriteLine("Hello World!");
        }
    }
}
