using Microsoft.Win32;
using Topshelf;

namespace LogonService.Utils;

public class Extensions
{
    private static IHost Host { get; set; }
    private static string ServiceName { get; set; }

    public static void RunWindowsServiceWithHost(IHost host)
    {
        Host = host;
        ServiceName = "DemoService";

        switch (ActivateTopshelf())
        {
            case TopshelfExitCode.Ok:
                Console.WriteLine($"{ServiceName} status: Ok");
                break;
            // 略...
            default:
                Console.WriteLine($"{ServiceName} status: Unsupported status...");
                break;
        }
    }

    private static TopshelfExitCode ActivateTopshelf() =>
        HostFactory.Run(configurator =>
        {
            // 設定執行時所傳入的啟動參數
            var env = string.Empty;
            configurator.AddCommandLineDefinition(nameof(env), value => { env = value; });
            configurator.ApplyCommandLine();

            // 設定啟動的主要邏輯程式
            // var app = Host.Services.GetRequiredService<App>();
            configurator.Service<IHost>(settings =>
            {
                settings.ConstructUsing(() => Host);
                settings.WhenStarted(app => app.Start());
                settings.BeforeStoppingService(service => { service.Stop(); });
                settings.WhenStopped(app => { app.StopAsync(); });
            });

            // 設定啟動 Windows Service 的身分
            configurator.RunAsLocalSystem()
                .StartAutomaticallyDelayed()
                .EnableServiceRecovery(rc => rc.RestartService(5));

            // 設定服務名稱及描述
            configurator.SetServiceName($"{ServiceName}");
            configurator.SetDisplayName($"{ServiceName}");
            configurator.SetDescription($"{ServiceName}");

            // 設定發生例外時的處理方式
            configurator.OnException((exception) => { Console.WriteLine(exception.Message); });

            // 安裝之後將啟動時所需要的引數寫入 Windows 註冊表中，讓下次啟動時傳遞同樣的引數
            configurator.AfterInstall(installHostSettings =>
            {
                using (var system = Registry.LocalMachine.OpenSubKey("System"))
                using (var currentControlSet = system.OpenSubKey("CurrentControlSet"))
                using (var services = currentControlSet.OpenSubKey("Services"))
                using (var service = services.OpenSubKey(installHostSettings.ServiceName, true))
                {
                    const string REG_KEY_IMAGE_PATH = "ImagePath";
                    var imagePath = service?.GetValue(REG_KEY_IMAGE_PATH);
                    service?.SetValue(REG_KEY_IMAGE_PATH, $"{imagePath} -env:{env}");
                }
            });
        });
}