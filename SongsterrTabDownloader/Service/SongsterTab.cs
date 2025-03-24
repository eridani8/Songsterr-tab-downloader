namespace TabDownloader.Service;

public record SongsterTab(string RevisionId, string DownloadUrl, string Artist, string Title, string Extension)
{
    public string GetFileName()
    {
        var fileName = $"{Artist} - {Title} ({RevisionId}).{Extension}";
        return Path.GetInvalidFileNameChars().Aggregate(fileName, (current, invalidChar) => current.Replace(invalidChar.ToString(), "_"));
    }
}

