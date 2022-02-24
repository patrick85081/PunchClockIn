using System.Diagnostics;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using CommandLine;
using LogonService.Models;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using NLog;
using NLog.Web;
using Topshelf;
using static System.IO.Path;

namespace LogonService;

public class Program
{
    public static async Task<int> Main(string[] args)
    {
        var exitCode = await Parser.Default.ParseArguments<InstallOption, UninstallOption, RunOption, Task<int>>(args)
            .MapResult(
                (InstallOption option) => InstallService(option),
                (UninstallOption option) => UninstallService(option),
                (RunOption option) => RunService(args),
                error => Task.FromResult(1)
            );
        return exitCode;
    }

    private static async Task<int> RunService(string[] args)
    {
        Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);
        await CreateHostBuilder(args)
            .Build()
            .RunAsync();
        return 0;
    }

    private static async Task<int> InstallService(InstallOption install)
    {
        var sb = new StringBuilder();
        sb.Append($"create start=auto");

        // Service Name
        if (string.IsNullOrEmpty(install.ServiceName))
            sb.Append($" LogonService");
        else
            sb.Append($" {install.ServiceName}");

        // Binary Path
        if (string.IsNullOrEmpty(install.ServicePath))
            sb.Append($" binPath={install.ServicePath}");
        else
            sb.Append($" binPath={ChangeExtension(Assembly.GetEntryAssembly().Location, "exe")}");
        
        // Account
        if (!string.IsNullOrEmpty(install.Account))
            sb.Append($" obj={install.Account}");
        
        // Password
        if (!string.IsNullOrEmpty(install.Password))
            sb.Append($" password={install.Password}");

        await RunProcess(
            "sc",
            sb.ToString());
        return 0;
    }

    private static async Task<int> UninstallService(UninstallOption install)
    {
        await RunProcess(
            "sc",
            $"delete LogonService");
        return 0;
    }

    private static IHostBuilder CreateHostBuilder(string[] args)
    {
        return Microsoft.Extensions.Hosting.Host.CreateDefaultBuilder(args)
            .UseWindowsService(option => option.ServiceName = "LogonService")
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

    private static async Task RunProcess(string fileName, string arguments)
    {
        var startInfo = new ProcessStartInfo(
            fileName,
            arguments)
        {
            RedirectStandardOutput = true,
        };
        var process = new Process()
        {
            StartInfo = startInfo,
        };
        process.Start();
        var output = process.StandardOutput;
        while (!output.EndOfStream)
            Console.WriteLine(await output.ReadLineAsync());
    }
}