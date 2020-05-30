using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace WindowsServiceWorker
{
    public class PollingService : IHostedService, IDisposable
    {
        private System.Timers.Timer _timer = new System.Timers.Timer(1000);

        public Task StartAsync(CancellationToken token)
        {
            _timer.Elapsed += Elapsed;
            _timer.Start();
            return Task.CompletedTask;
        }

        private void Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            Console.WriteLine("Execute");
        }

        public Task StopAsync(CancellationToken token)
        {
            _timer.Stop();
            _timer.Elapsed -= Elapsed;
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer.Dispose();
        }
    }
}