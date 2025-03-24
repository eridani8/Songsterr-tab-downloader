namespace TabDownloader.Service;

public record AppSettings
{
    public required string SiteUrl { get; init; }
    public required Dictionary<string, string> Headers { get; init; }
}