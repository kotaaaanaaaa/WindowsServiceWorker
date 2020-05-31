using System;
using System.Diagnostics;
using System.ServiceProcess;

namespace WindowsServiceWorker
{
    public static class ServiceAccessor
    {
        public static bool IsInstalled(string name)
        {
            var sc = new ServiceController(name);
            try
            {
                return sc.ServiceName == name;
            }
            catch (InvalidOperationException)
            {
                return false;
            }
        }

        public static void Install(string name, string path)
        {
            InstallService(name, path);
            FailurService(name, "restart", 1000, "restart", 1000, "restart", 1000);
            StartService(name);
        }

        public static void Uninstall(string name)
        {
            StopService(name);
            UninstallService(name);
        }

        private static void InstallService(string name, string path)
        {
            var proc = new Process();
            proc.StartInfo.FileName = @"sc.exe";
            proc.StartInfo.Arguments = $"Create {name} binPath=\"{path} --service {name}\" start=auto";
            proc.Start();
            proc.WaitForExit();
        }

        private static void StartService(string name)
        {
            var proc = new Process();
            proc.StartInfo.FileName = @"sc.exe";
            proc.StartInfo.Arguments = $"Start {name}";
            proc.Start();
            proc.WaitForExit();
        }

        private static void StopService(string name)
        {
            var proc = new Process();
            proc.StartInfo.FileName = @"sc.exe";
            proc.StartInfo.Arguments = $"Stop {name}";
            proc.Start();
            proc.WaitForExit();
        }

        private static void UninstallService(string name)
        {
            var proc = new Process();
            proc.StartInfo.FileName = @"sc.exe";
            proc.StartInfo.Arguments = $"Delete {name}";
            proc.Start();
            proc.WaitForExit();
        }

        private static void FailurService(string name, string act1, int time1, string act2, int time2, string act3, int time3)
        {
            var actions = $"{act1}/{time1}/{act2}/{time2}/{act3}/{time3}";
            var reset = $"{3600}";
            var proc = new Process();
            proc.StartInfo.FileName = @"sc.exe";
            proc.StartInfo.Arguments = $"Failure {name} actions= {actions} reset= {reset}";
            proc.Start();
            proc.WaitForExit();
        }
    }
}