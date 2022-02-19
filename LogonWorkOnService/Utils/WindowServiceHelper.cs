using System.Diagnostics;

namespace LogonWorkOnService.Utils;

public class WindowServiceHelper
{
    public static void InstallService(string title,
        string description)
    {
        string serviceFullFileName = Process.GetCurrentProcess().MainModule.FileName;
        string serviceName = Path.GetFileNameWithoutExtension(serviceFullFileName);

        InstallService(serviceFullFileName, serviceName, title, description);
    }

    public static void InstallService(string serviceFullFileName, string serviceName, string title,
        string description)
    {
        Process procCreateSC = new Process();
        procCreateSC.StartInfo = new ProcessStartInfo()
        {
            WindowStyle = ProcessWindowStyle.Hidden,
            FileName = "sc",
            Arguments = string.Format(
                @"create {0} binpath= ""{1} --service"" type= own start= auto DisplayName= ""{2}""", serviceName,
                serviceFullFileName, title)
        };
        procCreateSC.Start();

        Process procDescriptionSC = new Process();
        procDescriptionSC.StartInfo = new ProcessStartInfo()
        {
            WindowStyle = ProcessWindowStyle.Hidden,
            FileName = "sc",
            Arguments = string.Format(@"description {0} ""{1}""", serviceName, description)
        };
        procDescriptionSC.Start();
    }

    public static void Uninstall()
    {
        string strFullFileName = Process.GetCurrentProcess().MainModule.FileName;
        string strServiceName = Path.GetFileNameWithoutExtension(strFullFileName);

        Uninstall(strServiceName);
    }


    public static void Uninstall(string serviceName)
    {
        //string strFullFileName = Process.GetCurrentProcess().MainModule.FileName;
        //string strServiceName = Path.GetFileNameWithoutExtension(strFullFileName);
        Process proc = new Process();
        proc.StartInfo = new ProcessStartInfo()
        {
            WindowStyle = ProcessWindowStyle.Hidden,
            FileName = "sc",
            Arguments = string.Format(@"delete {0}", serviceName)
        };
        proc.Start();
    }

    public static void StartService()
    {

    }
}