using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace WindowsServiceWorker
{
    public class WrappingService : IHostedService, IDisposable
    {
        public Task StartAsync(CancellationToken token)
        {
            Console.WriteLine("Execute");
            return Task.CompletedTask;
        }
        public Task StopAsync(CancellationToken token)
        {

            return Task.CompletedTask;
        }

        public void Dispose()
        {
        }
    }
}