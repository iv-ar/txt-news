using System.Security.Cryptography;
using System.Text;
using AngleSharp.Dom;
using AngleSharp.Html.Parser;
using I2R.LightNews.Utilities;

namespace I2R.LightNews.Services;

public class GrabberService
{
    private readonly ILogger<GrabberService> _logger;
    private readonly HttpClient _http;
    private const string NrkPrefix = "nrkno";
    private const int StaleTime = 100800;

    private static AppPath _cachePath => new() {
        HostPath = "AppData/__sitecache"
    };

    public GrabberService(ILogger<GrabberService> logger, HttpClient http) {
        _logger = logger;
        _http = http;
    }

    public async Task<NewsArticle> GrabNrkArticleAsync(string url) {
        var strippedUrl = url.Replace("https://", "")
            .Replace("http://", "")
            .Replace("www.", "");

        if (!strippedUrl.StartsWith("nrk.no")
            || strippedUrl.StartsWith("nrk.no/mat")
            || strippedUrl.StartsWith("nrk.no/tv")
            || strippedUrl.StartsWith("nrk.no/radio")
            || strippedUrl.StartsWith("nrk.no/xl")
           ) return default;

        using var md5 = MD5.Create();
        var articleFilePrefix = "art-" + NrkPrefix + "-" + Convert.ToHexString(md5.ComputeHash(Encoding.UTF8.GetBytes(url)));
        var source = await GrabSourceAsync(url, articleFilePrefix);
        var parser = new HtmlParser();
        var doc = await parser.ParseDocumentAsync(source.Content);
        var result = new NewsArticle() {
            CachedAt = source.CacheFileCreatedAt,
            Href = url,
            Title = doc.QuerySelector("h1.title")?.TextContent,
            Subtitle = doc.QuerySelector(".article-lead p")?.TextContent,
            Authors = new List<NewsArticle.Author>()
        };

        foreach (var authorNode in doc.QuerySelectorAll(".authors .author")) {
            var author = new NewsArticle.Author() {
                Name = authorNode.QuerySelector(".author__name")?.TextContent,
                Title = authorNode.QuerySelector(".author__role")?.TextContent
            };
            result.Authors.Add(author);
        }

        DateTime.TryParse(doc.QuerySelector("time.datePublished")?.Attributes["datetime"]?.Value, out var published);
        DateTime.TryParse(doc.QuerySelector("time.dateModified")?.Attributes["datetime"]?.Value, out var modified);
        result.UpdatedAt = modified;
        result.PublishedAt = published;
        if (doc.QuerySelector("kortstokk-app") != default) {
            result.Content = HtmlSanitiser.SanitizeHtmlFragment(doc.QuerySelector(".dhks-cardSection").InnerHtml, ".dhks-background,.dhks-actions,.dhks-credits,.dhks-sticky-reset,.dhks-byline");
        } else {
            result.Content = HtmlSanitiser.SanitizeHtmlFragment(doc.QuerySelector(".article-body").InnerHtml, "a,.section-reference,.widget,.article-body--updating,.video-reference,.image-reference,.reference");
        }

        return result;
    }

    public async Task<NewsSource> GrabNrkAsync() {
        var source = await GrabSourceAsync("https://nrk.no", NrkPrefix);
        var parser = new HtmlParser();
        var doc = await parser.ParseDocumentAsync(source.Content);
        var result = new NewsSource() {
            Name = "nrk",
            Attribution = "Fra https://nrk.no",
            Created = source.CacheFileCreatedAt.DateTime,
            CanonicalUrl = doc.QuerySelector("link[rel='canonical']")?.Attributes["href"]?.Value ?? "uvisst",
            Articles = new List<NewsArticle>()
        };

        foreach (var articleAnchorNode in doc.QuerySelectorAll("main section a")) {
            var article = new NewsArticle {
                Href = articleAnchorNode.Attributes["href"]?.Value.Trim(),
                Title = articleAnchorNode.QuerySelector(".kur-room__title span")?.TextContent.Trim()
            };

            if (
                article.Href.IsNullOrWhiteSpace()
                || article.Title.IsNullOrWhiteSpace()
                || (!article.Href?.StartsWith("https://www.nrk.no") ?? true)
                || (article.Href?.StartsWith("https://www.nrk.no/mat") ?? false)
            ) {
                continue;
            }

            result.Articles.Add(article);
        }

        return result;
    }

    private class SourceResult
    {
        public string CacheFileName { get; set; }
        public string Content { get; set; }
        public DateTimeOffset CacheFileCreatedAt { get; set; }
    }

    private async Task<SourceResult> GrabSourceAsync(string url, string prefix, bool forceRefresh = false) {
        var cacheFileName = forceRefresh ? default : GetLatestCacheFile(prefix);
        if (cacheFileName != default) {
            _logger.LogInformation("Returned cached {0} file, filename: {1}", url, cacheFileName.CacheFileName);
            cacheFileName.Content = await File.ReadAllTextAsync(_cachePath.GetHostPathForFilename(cacheFileName.CacheFileName));
            return cacheFileName;
        }

        var sourceResponse = await _http.GetAsync(url);
        var sourceContent = await sourceResponse.Content.ReadAsStringAsync();
        var utcNow = DateTimeOffset.UtcNow;
        var newCacheFileName = prefix + "-" + utcNow.ToUnixTimeSeconds() + ".html";
        await File.WriteAllTextAsync(_cachePath.GetHostPathForFilename(newCacheFileName), sourceContent);
        _logger.LogInformation("Wrote new cache file for {0}, filename: {1}", url, newCacheFileName);
        return new SourceResult() {
            CacheFileName = newCacheFileName,
            CacheFileCreatedAt = utcNow,
            Content = sourceContent
        };
    }

    private SourceResult GetLatestCacheFile(string prefix) {
        var cacheDirectoryInfo = new DirectoryInfo(_cachePath.HostPath);
        if (!cacheDirectoryInfo.Exists) {
            cacheDirectoryInfo.Create();
            return default;
        }

        var files = cacheDirectoryInfo.GetFiles();
        if (!files.Any()) return default;
        var relevantFiles = files.Where(c => c.Name.StartsWith(prefix)).OrderBy(c => c.Name).ToList();
        if (!relevantFiles.Any()) return default;
        var mostRecentFileName = relevantFiles.Last().Name;
        var mostRecentEpochString = new string(mostRecentFileName.Skip(mostRecentFileName.LastIndexOf('-')).Where(Char.IsDigit).ToArray());
        long.TryParse(mostRecentEpochString, out var mostRecentEpochLong);
        // more than 30 minutes since last grab
        if (mostRecentEpochLong + StaleTime < DateTimeOffset.UtcNow.ToUnixTimeSeconds()) return default;
        return new SourceResult {
            CacheFileName = mostRecentFileName,
            CacheFileCreatedAt = DateTimeOffset.FromUnixTimeSeconds(mostRecentEpochLong)
        };
    }
}