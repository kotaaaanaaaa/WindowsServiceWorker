using Microsoft.Extensions.Hosting;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace WindowsServiceWorker
{
    public class ServiceWorker : IHostedService, IDisposable
    {
        private string BasePath { get; set; }

        private string ServiceName { get; set; }

        private string ServiceExe { get; set; }

        private string ServiceArg { get; set; }

        private bool IsPolling { get; set; }

        private System.Timers.Timer Timer { get; set; }

        private Process RunningProcess { get; set; }

        private bool IsElapsing { get; set; }

        public Task StartAsync(CancellationToken token)
        {
            BasePath = ServiceContext.Instance().Get("BasePath");
            ServiceName = ServiceContext.Instance().Get("ServiceName");
            ServiceExe = ServiceContext.Instance().GetService(ServiceName, "Exe");
            ServiceArg = ServiceContext.Instance().GetService(ServiceName, "Arg");
            IsPolling = ServiceContext.Instance().GetService(ServiceName, "Type") == "Polling";

            ServiceArg = ServiceArg.Replace(@"%EXEDIR%",BasePath);

            IsElapsing = false;

            if (IsPolling)
            {
                var interval = int.Parse(ServiceContext.Instance().GetService(ServiceName, "Interval"));
                Timer = new System.Timers.Timer(interval);
                Timer.Elapsed += Pooling;
                Timer.Start();
            }
            else
            {
                RunningProcess = Setup();
                RunningProcess.Start();

                var interval = 1000;
                Timer = new System.Timers.Timer(interval);
                Timer.Elapsed += HealthCheck;
                Timer.Start();
            }

            return Task.CompletedTask;
        }

        private void Pooling(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (RunningProcess.HasExited && !IsElapsing)
            {
                IsElapsing = true;
                RunningProcess = Setup();
                RunningProcess.Start();
                IsElapsing = false;
            }
        }

        private void HealthCheck(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (RunningProcess.HasExited && !IsElapsing)
            {
                IsElapsing = true;
                RunningProcess = Setup();
                RunningProcess.Start();
                IsElapsing = false;
            }
        }

        public Process Setup()
        {
            var prc = new Process();
            prc.StartInfo.WorkingDirectory = BasePath;
            prc.StartInfo.FileName = ServiceExe;
            prc.StartInfo.Arguments = ServiceArg;
            return prc;
        }
        
        public Task StopAsync(CancellationToken token)
        {
            Timer.Stop();
            Timer.Elapsed -= Pooling;
            Timer.Elapsed -= HealthCheck;

            RunningProcess.WaitForExit(10000);
            if (!RunningProcess.HasExited)
            {
                RunningProcess.Kill();
            }

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            Timer?.Dispose();
            RunningProcess.Dispose();
        }
    }
}