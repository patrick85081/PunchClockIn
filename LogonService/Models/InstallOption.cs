using CommandLine;

namespace LogonService.Models;

[Verb("install", HelpText = "Install Windows Service")]
public class InstallOption
{
    [CommandLine.Option('b', "binPath", Required = false, HelpText = "Service exe path setting")]
    public string ServicePath { get; set; }
        
    [CommandLine.Option('n', "name", Required = false, HelpText = "Service Name setting")]
    public string ServiceName { get; set; }
        
    [CommandLine.Option('u', "user", Required = false, HelpText = "Service Account Setting")]
    public string Account { get; set; }
        
    [CommandLine.Option('p', "password", Required = false, HelpText = "Service Password Setting")]
    public string Password { get; set; }
}