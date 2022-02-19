using System.Net.Sockets;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using NLog;
using NLog.Web;
using Topshelf;

namespace LogonWorkOnService;

public class Program
{
    // static string[] TopshelfArgs = new string[]{"install","help", "uninstall", "run","stop"};
    public static int Main(string[] args)
    {
        Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);
        CreateHostBuilder(args)
            .Build()
            .Run();
        return 0;

        /*
        var rc = Topshelf.HostFactory.Run(x =>
        {
            x.UseNLog();
            x.Service<IHost>(s =>
            {
                s.ConstructUsing(name =>  CreateHostBuilder(args).Build());
                s.WhenStarted((tc) => tc.Run());
                s.WhenStopped(tc => tc.StopAsync());
            });
            x.RunAsLocalSystem();
            x.StartAutomatically();
            
            x.EnableServiceRecovery(r => r.RestartService(TimeSpan.FromSeconds(10)));
            
            x.SetServiceName("LogoWorkOnService");
        });

        var exitCode = (int)Convert.ChangeType(rc, rc.GetTypeCode());
        return exitCode;
        */
    }

    private static IHostBuilder CreateHostBuilder(string[] args)
    {
        return Microsoft.Extensions.Hosting.Host.CreateDefaultBuilder(args)
            .UseWindowsService(option => option.ServiceName = "LogonWorkOnService")
            .UseNLog()
            .ConfigureServices(service =>
            {
                service.AddHttpClient<HttpClient>("WorkOn",
                        client => { client.BaseAddress = new Uri("http://localhost:5000"); })
                    .ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler()
                    {
                        ConnectCallback = async (context, token) =>
                        {
                            var socket = new Socket(AddressFamily.Unix, SocketType.Stream, ProtocolType.IP);
                            var socketPath = Path.Combine(Path.GetTempPath(), "PunchClockIn", "Socket.sock");
                            service.BuildServiceProvider()
                                .GetService<ILoggerFactory>()
                                .CreateLogger<Program>()
                                .LogInformation($"Listen Socket File : {socketPath}");
                            var endPoint = new UnixDomainSocketEndPoint(socketPath);
                            await socket.ConnectAsync(endPoint, token);
                            return new NetworkStream(socket, true);
                        },
                    });
            })
            .ConfigureServices(services => services.AddHostedService<Worker>());
    }
}