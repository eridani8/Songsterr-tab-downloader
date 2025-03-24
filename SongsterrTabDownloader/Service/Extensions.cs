using HtmlAgilityPack;
using Spectre.Console;

namespace TabDownloader.Service;

public static class Extensions
{
    public static string GetTabString()
    {
        return "       ";
    }
    
    public static string MarkupAquaColor(this string str)
    {
        return $"[aquamarine1]{str}[/]";
    }

    public static string MarkupMainColor(this string str)
    {
        return $"[mediumorchid3]{str}[/]";
    }

    public static string MarkupErrorColor(this string str)
    {
        return $"[red3_1]{str}[/]";
    }
    
    public static async Task<HtmlDocument?> GetHtmlDocument(this Task<string> html)
    {
        var content = await html;
        if (string.IsNullOrEmpty(content)) return null;
        var htmlDocument = new HtmlDocument
        {
            OptionFixNestedTags = true
        };
        htmlDocument.LoadHtml(content);
        return htmlDocument;
    }
}