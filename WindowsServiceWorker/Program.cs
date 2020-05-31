using Microsoft.Extensions.CommandLineUtils;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection.Extensions;

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
                optionType: CommandOptionType.SingleValue);

            var consoleOption = app.Option(
                template: "--console",
                description: "コンソールとして実行します",
                optionType: CommandOptionType.SingleValue);

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
                SetServiceContext();

                var builder = CreateHostBuilder(args);
                if (serviceOption.HasValue())
                {
                    ServiceContext.Instance().Set("ServiceName", serviceOption.Value());
                    Task.Run(async () => { await builder.RunAsServiceAsync(); }).Wait();
                    return 0;
                }
                if (consoleOption.HasValue() || Debugger.IsAttached)
                {
                    ServiceContext.Instance().Set("ServiceName", consoleOption.Value());
                    Task.Run(async () => { await builder.RunConsoleAsync(); }).Wait();
                    return 0;
                }

                ServiceContext.Instance().Services.ToList().ForEach(x =>
                {
                    var name = x.Key;
                    var path = Process.GetCurrentProcess().MainModule.FileName;
                    var disp = ServiceContext.Instance().GetService(name, "Display");
                    var desc = ServiceContext.Instance().GetService(name, "Description");

                    if (installOption.HasValue())
                    {
                        ServiceAccessor.Install(name, path, disp, desc);
                    }
                    else if (uninstallOption.HasValue())
                    {
                        ServiceAccessor.Uninstall(name);
                    }
                    else if (ServiceAccessor.IsInstalled(name))
                    {
                        var process = new Process();
                        process.StartInfo.FileName = path;
                        process.StartInfo.Arguments = @"--uninstall";
                        process.StartInfo.Verb = "RunAs";
                        process.StartInfo.UseShellExecute = true;
                        process.Start();
                        process.WaitForExit();
                    }
                    else
                    {
                        var process = new Process();
                        process.StartInfo.FileName = path;
                        process.StartInfo.Arguments = @"--install";
                        process.StartInfo.Verb = "RunAs";
                        process.StartInfo.UseShellExecute = true;
                        process.Start();
                        process.WaitForExit();
                    }
                });

                return 0;
            });

            app.Execute(args);
        }

        private static IHostBuilder CreateHostBuilder(string[] args) =>
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
                    services.AddHostedService<ServiceWorker>();
                });

        private static IConfiguration Configuration()
        {
            var configBuilder = new ConfigurationBuilder();
            configBuilder.SetBasePath(Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName));
            configBuilder.AddJsonFile(@"Config.json");

            return configBuilder.Build();
        }

        private static int Count(IConfiguration conf, string name)
        {
            var sections = conf.GetSection(name).AsEnumerable();
            return sections.Count(x => Regex.IsMatch(x.Key, $"^{name}:\\d$"));
        }

        private static void SetServiceContext()
        {
            var conf = Configuration();
            var services = new Dictionary<string, Dictionary<string, string>>();
            Enumerable.Range(1, Count(conf, "Services"))
                .ToList()
                .ForEach(x =>
                {
                    var name = conf.GetSection($"Services:{x - 1}")["Name"];
                    var val = new Dictionary<string, string>();
                    conf.GetSection($"Services:{x - 1}")
                        .AsEnumerable()
                        .Where(kv => kv.Key != $"Services:{x - 1}")
                        .ToList()
                        .ForEach(kv =>
                        {
                            var key = kv.Key.Substring($"Services:{x - 1}:".Length);
                            if (key != "Name")
                            {
                                ServiceContext.Instance().SetService(name, key, kv.Value);
                            }
                        });
                });
        }
    }
}
