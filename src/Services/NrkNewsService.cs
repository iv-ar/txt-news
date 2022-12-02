using System.Security.Cryptography;
using System.Text;
using AngleSharp.Html.Parser;
using I2R.LightNews.Utilities;
using Microsoft.Extensions.Caching.Memory;

namespace I2R.LightNews.Services;

public class NrkNewsService
{
    private readonly ILogger<NrkNewsService> _logger;
    private readonly IMemoryCache _memoryCache;
    private readonly HttpClient _http;
    private const string NrkPrefix = "nrkno";
    private const int StaleTime = 1800;

    private static AppPath _cachePath => new() {
        HostPath = "AppData/__sitecache"
    };

    public NrkNewsService(ILogger<NrkNewsService> logger, HttpClient http, IMemoryCache memoryCache) {
        _logger = logger;
        _http = http;
        _memoryCache = memoryCache;
    }

    private bool IsSupportedNrkUrl(string url) {
        var strippedUrl = url.Replace("https://", "")
            .Replace("http://", "")
            .Replace("www.", "");

        var ignored = new List<string>() {
            "nrk.no/mat",
            "nrk.no/radio",
            "nrk.no/tv",
            "nrk.no/video",
            "nrk.no/podkast"
        };

        return strippedUrl.StartsWith("nrk.no") && ignored.All(c => !strippedUrl.Contains(c));
    }

    public async Task<NewsArticle> GrabNrkArticleAsync(string url) {
        if (!IsSupportedNrkUrl(url)) return default;
        using var md5 = MD5.Create();
        var articleFilePrefix = "art-" + NrkPrefix + "-" + Convert.ToHexString(md5.ComputeHash(Encoding.UTF8.GetBytes(url)));
        return await _memoryCache.GetOrCreateAsync(articleFilePrefix, async entry => {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10);
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

            var defaultExcludes = new List<string>() {
                ".dhks-background",
                ".dhks-actions",
                ".dhks-credits",
                ".dhks-sticky-reset",
                ".dhks-byline",
                ".compilation-reference",
                ".section-reference",
                ".image",
                ".fact__expand",
                ".image-reference",
                ".video-reference",
                ".article-body--updating",
                ".external-reference",
                ".reference",
                ".atlas-reference",
                ".remoterenderedcontent-reference",
                "text:Følg utviklingen i NRKs Nyhetssenter",
                "text:Bli med i debatten under"
            };

            if (doc.QuerySelector("kortstokk-app") != default) {
                result.Title = doc.QuerySelector(".dhks-title span")?.TextContent;
                result.Content = HtmlSanitiser.SanitizeHtmlFragment(doc.QuerySelector(".dhks-cardSection").InnerHtml, string.Join(',', defaultExcludes));
            } else if (url.Contains("/xl/")) {
                var subtitle = doc.QuerySelector(".article-feature__intro p").InnerHtml;
                result.Title = doc.QuerySelector(".article-feature__intro h1").TextContent;
                var contentHtml = doc.QuerySelector(".article-feature__body").InnerHtml;
                result.Content = HtmlSanitiser.SanitizeHtmlFragment(subtitle + contentHtml, string.Join(',', defaultExcludes));
            } else if (url.Contains("nrk.no/nyheter") || (doc.QuerySelector(".bulletin-text") != default && doc.QuerySelector(".article-body") == defaultExcludes)) {
                result.Content = HtmlSanitiser.SanitizeHtmlFragment(doc.QuerySelector(".bulletin-text").InnerHtml);
            } else {
                result.Content = HtmlSanitiser.SanitizeHtmlFragment(doc.QuerySelector(".article-body").InnerHtml, string.Join(',', defaultExcludes));
            }

            return result;
        });
    }

    public async Task<NewsSource> GrabNrkAsync() {
        return await _memoryCache.GetOrCreateAsync(NrkPrefix, async entry => {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(31);
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

                if (article.Href.IsNullOrWhiteSpace() || article.Title.IsNullOrWhiteSpace() || !IsSupportedNrkUrl(article.Href)) {
                    continue;
                }

                result.Articles.Add(article);
            }

            return result;
        });
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