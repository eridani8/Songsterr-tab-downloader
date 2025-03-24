using Flurl.Http;
using Microsoft.Extensions.Options;
using Serilog;
using Spectre.Console;

namespace TabDownloader.Service;

public class MenuHandler(SongsterParser songsterParser, IOptions<AppSettings> settings, CookieJar cookies, Style style)
{
    public async Task GetFirstCookies()
    {
        await settings.Value.SiteUrl
            .WithHeaders(settings.Value.Headers)
            .WithCookies(cookies)
            .GetStringAsync();
    }
    
    public async Task DownloadTabs()
    {
        var searchUrl = GetUrlFromUser();

        var tabUrls = await songsterParser.ParseSearchUrls(searchUrl);
        if (tabUrls != null)
        {
            foreach (var (index, tabUrl) in tabUrls.Index())
            {
                await DownloadTab(true, tabUrl);
                await Task.Delay(500);
                AnsiConsole.Write(Extensions.GetTabString().EscapeMarkup());
                AnsiConsole.Markup($"{index + 1}/{tabUrls.Count}".MarkupMainColor());
                AnsiConsole.WriteLine();
            }
        }
    }

    public async Task DownloadTab(bool isMultiple, string? tabUrl = null)
    {
        tabUrl ??= GetUrlFromUser();

        var tab = await songsterParser.ParseTab(tabUrl);
        if (tab is null)
        {
            Log.ForContext<MenuHandler>().Error("tab is not parsed");
            return;
        }
        
        var folder = Path.Join("Tabs", tab.Artist);
        var safeFileName = tab.GetFileName();
        var filePath = await tab.DownloadUrl
            .WithHeaders(settings.Value.Headers)
            .WithCookies(cookies)
            .DownloadFileAsync(folder, safeFileName);
        
        if (string.IsNullOrEmpty(filePath))
        {
            Log.ForContext<MenuHandler>().Error("tab is not downloaded");
            return;
        }
        
        AnsiConsole.Write(new TextPath(filePath.EscapeMarkup())
            .RootColor(Color.Yellow)
            .SeparatorColor(Color.SeaGreen1)
            .StemColor(Color.Yellow)
            .LeafColor(Color.Green));
        if (!isMultiple)
        {
            AnsiConsole.Write(Extensions.GetTabString().EscapeMarkup());
            AnsiConsole.Markup("[mediumorchid3]OK[/]");
            AnsiConsole.WriteLine();
        }
    }
    
    public string GetUrlFromUser()
    {
        var url = AnsiConsole.Prompt(
            new TextPrompt<string>($"{"enter a".MarkupMainColor()} {"link".MarkupAquaColor()}")
                .PromptStyle(style)
                .ValidationErrorMessage("entered link is incorrect".MarkupErrorColor())
                .Validate(url =>
                    Uri.IsWellFormedUriString(url, UriKind.Absolute)));

        return url;
    }
}