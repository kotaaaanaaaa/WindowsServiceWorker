using Microsoft.Extensions.CommandLineUtils;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace WindowsServiceWorker
{
    class Program
    {
        static void Main(string[] args)
        {
            var app = new CommandLineApplication();
            app.Name = "Windows Service Worker";
            app.Description = "Windows Serviceとして実行します。";
            app.HelpOption("-h | --help");

            var serviceOption = app.Option(
                template: "--service",
                description: "サービスとして実行します",
                optionType: CommandOptionType.NoValue);

            var consoleOption = app.Option(
                template: "--console",
                description: "コンソールとして実行します",
                optionType: CommandOptionType.NoValue);

            var installOption = app.Option(
                template: "--install",
                description: "サービスをインストールします",
                optionType: CommandOptionType.NoValue);

            var uninstallOption = app.Option(
                template: "--uninstall",
                description: "サービスをアンインストールします",
                optionType: CommandOptionType.NoValue);

            app.OnExecute(() =>
            {
                var name = "WindowsServiceWorkerTest";
                var basePath = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);
                var path = Path.Combine(basePath, "WindowsServiceWorker.exe");

                var builder = CreateHostBuilder(args);
                if (serviceOption.HasValue())
                {
                    Task.Run(async () => { await builder.RunAsServiceAsync(); }).Wait();
                    return 0;
                }

                if (consoleOption.HasValue() || Debugger.IsAttached)
                {
                    Task.Run(async () => { await builder.RunConsoleAsync(); }).Wait();
                    return 0;
                }

                if (installOption.HasValue())
                {
                    ServiceAccessor.Install(name, path);
                    return 0;
                }

                if (uninstallOption.HasValue())
                {
                    ServiceAccessor.Uninstall(name);
                    return 0;
                }

                if (ServiceAccessor.IsInstalled(name))
                {
                    var process = new Process();
                    process.StartInfo.FileName = @"WindowsServiceWorker.exe";
                    process.StartInfo.Arguments = @"--uninstall";
                    process.StartInfo.Verb = "RunAs";
                    process.StartInfo.UseShellExecute = true;
                    process.Start();
                    process.WaitForExit();
                }
                else
                {
                    var process = new Process();
                    process.StartInfo.FileName = @"WindowsServiceWorker.exe";
                    process.StartInfo.Arguments = @"--install";
                    process.StartInfo.Verb = "RunAs";
                    process.StartInfo.UseShellExecute = true;
                    process.Start();
                    process.WaitForExit();
                }

                return 0;
            });

            app.Execute(args);
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            new HostBuilder()
                .ConfigureAppConfiguration((context, builder) =>
                {
                    context.HostingEnvironment.EnvironmentName =
                        System.Environment.GetEnvironmentVariable("NETCORE_ENVIRONMENT") ?? "production";
                    builder.SetBasePath(Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName));
                    builder.AddCommandLine(args);
                })
                .ConfigureServices((context, services) =>
                {
                    services.AddLogging();
                    services.AddHostedService<PollingService>();
                });
    }
}
