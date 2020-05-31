using Microsoft.Extensions.Hosting;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace WindowsServiceWorker
{
    public class ServiceWorker : IHostedService, IDisposable
    {
        private string ServiceName { get; set; }

        private string ServiceExe { get; set; }

        private string ServiceArg { get; set; }

        private bool IsPolling { get; set; }

        private System.Timers.Timer Timer { get; set; }

        private bool isExecuting = false;

        public Task StartAsync(CancellationToken token)
        {
            ServiceName = ServiceContext.Instance().Get("ServiceName");
            ServiceExe = ServiceContext.Instance().GetService(ServiceName, "Exe");
            ServiceArg = ServiceContext.Instance().GetService(ServiceName, "Arg");
            IsPolling = ServiceContext.Instance().GetService(ServiceName, "Type") == "Polling";

            if (IsPolling)
            {
                var interval = int.Parse(ServiceContext.Instance().GetService(ServiceName, "Interval"));
                Timer = new System.Timers.Timer(interval);
                Timer.Elapsed += Elapsed;
                Timer.Start();
            }
            Main();

            return Task.CompletedTask;
        }

        private void Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (!isExecuting)
            {
                isExecuting = true;
                Main();
                isExecuting = false;
            }
        }

        public void Main()
        {
            var process = new Process();
            process.StartInfo.FileName = ServiceExe;
            process.StartInfo.Arguments = ServiceArg;
            process.Start();
            process.WaitForExit();
        }

        public Task StopAsync(CancellationToken token)
        {
            if (IsPolling)
            {
                Timer.Stop();
                Timer.Elapsed -= Elapsed;
            }

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            Timer?.Dispose();
        }
    }
}