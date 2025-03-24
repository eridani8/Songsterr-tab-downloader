using System.Diagnostics;
using Microsoft.Extensions.Hosting;
using Serilog;
using Spectre.Console;

namespace TabDownloader.Service;

public class ConsoleMenu(MenuHandler menuHandler, IHostApplicationLifetime lifetime, Style style) : IHostedService
{
    private Task? _task;
    
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _task = Worker();
        await menuHandler.GetFirstCookies();
    }
    public async Task StopAsync(CancellationToken cancellationToken)
    {
        try
        {
            lifetime.StopApplication();
            if (_task != null)
            {
                await Task.WhenAny(_task, Task.Delay(Timeout.Infinite, cancellationToken));
            }
        }
        finally
        {
            _task?.Dispose();
        }
    }

    private async Task Worker()
    {
        const string downloadSingleTab = "download single tab";
        const string downloadMultipleTabs = "download multiple tabs";
        const string openTabsFolder = "open tabs folder";
        const string exit = "exit";
        
        while (!lifetime.ApplicationStopping.IsCancellationRequested)
        {
            var choices = new SelectionPrompt<string>()
                .Title("Choose an action")
                .HighlightStyle(style)
                .AddChoices(downloadSingleTab, downloadMultipleTabs, openTabsFolder, exit);
            
            var prompt = AnsiConsole.Prompt(choices);

            try
            {
                switch (prompt)
                {
                    case downloadSingleTab:
                        await menuHandler.DownloadTab(false);
                        break;
                    case downloadMultipleTabs:
                        await menuHandler.DownloadTabs();
                        break;
                    case openTabsFolder:
                        Process.Start(new ProcessStartInfo("Tabs") { UseShellExecute = true });
                        break;
                    case exit:
                        lifetime.StopApplication();
                        break;
                }
            }
            catch (Exception e)
            {
                Log.ForContext<ConsoleMenu>().Error(e, "An error occurred while selecting the menu item");
            }
        }
    }
}