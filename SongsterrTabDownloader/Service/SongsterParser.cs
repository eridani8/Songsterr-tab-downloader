using System.Xml;
using Flurl.Http;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;

namespace TabDownloader.Service;

public class SongsterParser(IOptions<AppSettings> settings, CookieJar cookies)
{
    public async Task<List<string>?> ParseSearchUrls(string url)
    {
        if (string.IsNullOrEmpty(url)) return null;
        if (!url.StartsWith(settings.Value.SiteUrl)) return null;
        
        var doc = await url
            .WithHeaders(settings.Value.Headers)
            .WithCookies(cookies)
            .GetStringAsync()
            .GetHtmlDocument();

        var tabs = doc?.DocumentNode.SelectNodes("//div[@id='search-wrap']//div[@*='songs']/a|//div[@id='artist-wrap']//div[@data-list='artist']/a");
        if (tabs == null) return null;

        var result = new List<string>();
        
        foreach (var tabNode in tabs)
        {
            var tabUrl = settings.Value.SiteUrl + tabNode.Attributes["href"].Value;
            if (!tabUrl.EndsWith(settings.Value.SiteUrl))
            {
                result.Add(tabUrl);
            }
        }
        
        return result;
    }
    public async Task<SongsterTab?> ParseTab(string url)
    {
        if (string.IsNullOrEmpty(url)) return null;
        if (!url.StartsWith(settings.Value.SiteUrl)) return null;

        var doc = await url
            .WithHeaders(settings.Value.Headers)
            .WithCookies(cookies)
            .GetStringAsync()
            .GetHtmlDocument();

        if (doc == null) return null;

        var script = doc.DocumentNode.SelectSingleNode("//script[@id='state']").InnerHtml;
        var json = JObject.Parse(script);
        
        var selectToken = json.SelectToken("meta.current");
        var revisionId = selectToken?["revisionId"]?.ToString();
        var artist = selectToken?["artist"]?.ToString();
        var title = selectToken?["title"]?.ToString();

        var tabUrl = selectToken?["source"]?.ToString();
        var tabExt = tabUrl?.Split('.').Last();

        if (revisionId == null || artist == null || title == null || tabUrl == null || tabExt == null)
        {
            return null;
        }
        
        return new SongsterTab(revisionId, tabUrl, artist, title, tabExt);
    }
}