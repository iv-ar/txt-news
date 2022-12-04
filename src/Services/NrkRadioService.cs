using Microsoft.Extensions.Caching.Memory;

namespace I2R.LightNews.Services;

public class NrkRadioService
{
    private readonly IMemoryCache _cache;
    private readonly HttpClient _http;
    private const string CATEGORY_SEARCH_CACHE_KEY = "category_search";
    private readonly ILogger<NrkRadioService> _logger;

    public NrkRadioService(IMemoryCache cache, HttpClient http, ILogger<NrkRadioService> logger) {
        _cache = cache;
        http.BaseAddress = new Uri("https://psapi.nrk.no");
        _http = http;
        _logger = logger;
    }

    public async Task CreateIndex() {
        var letters = "ABCDEFGHIJKLMNOPQRSTUVWXYZÆØÅ".ToCharArray();
        var skip = 0;
        foreach (var letter in letters) {
            var path = "/radio/search/categories/alt-innhold?letter=" + letter + "&skip=0&take=50";
            while (path.HasValue()) {
                var response = await _http.GetFromJsonAsync<RadioCategorySearchResult>(path);
                if (response == default) break;
                await Task.Delay(2000);
                foreach (var series in response.Series) {
                    var dbSeries = new RadioSeries {
                        Name = series.Title,
                        NrkId = series.Id,
                        Type = series.Type,
                    };
                    var seriesId = RadioIndexDb.AddSeries(dbSeries);
                    _logger.LogInformation("Added series {0} with id {1}, to the database", dbSeries.Name, seriesId);
                    if (!series.Links.Series.Href.HasValue()) continue;
                    var seriesMetadata = await _http.GetFromJsonAsync<NrkRadioSeries>(series.Links.Series.Href);
                    if (seriesMetadata == default) continue;
                    await Task.Delay(1000);
                    foreach (var season in seriesMetadata.Embedded.Seasons) {
                        var dbSeason = new RadioSeason() {
                            Name = season.Titles.FirstOrDefault()?.Title,
                            NrkId = season.Id,
                            SeriesId = seriesId
                        };
                        var seasonId = RadioIndexDb.AddSeason(dbSeason);
                        _logger.LogInformation("Added season {0} to series {1} with id {2}, to the database", dbSeason.Name, dbSeries.Name, seasonId);
                        foreach (var episode in season.Episodes) {
                            foreach (var actuallyEpisode in episode.Embedded.Episodes) {
                                var dbEpisode = new RadioEpisode {
                                    CanonicalUrl = actuallyEpisode.Links.Share.Href,
                                    Title = actuallyEpisode.Titles.FirstOrDefault()?.Title,
                                    Subtitle = actuallyEpisode.Titles.FirstOrDefault()?.Subtitle,
                                    NrkId = actuallyEpisode.EpisodeId,
                                    SeasonId = seasonId,
                                    SeriesId = dbSeason.SeriesId
                                };
                                var playbackResponse = await _http.GetFromJsonAsync<NrkPlaybackManifest>("/playback/manifest/program/" + dbEpisode.NrkId);
                                if (playbackResponse == default) continue;
                                dbEpisode.SourceUrl = playbackResponse.Playable.Assets.FirstOrDefault()?.Url;
                                var episodeId = RadioIndexDb.AddEpisode(dbEpisode);
                                _logger.LogInformation("Added episode {0} to series {1} season {2} with id {3}, to the database", dbEpisode.Title, dbSeries.Name, dbSeason.Name, episodeId);
                            }
                        }
                    }
                }

                path = response.Links.NextPage.Href.HasValue() ? response.Links.NextPage.Href : "";
            }
        }
    }

    public async Task<RadioCategorySearchResult> SearchCategoriesAsync(string query, int take = 50, int skip = 0) {
        return await _http.GetFromJsonAsync<RadioCategorySearchResult>(
            "/radio/search/categories/alt-innhold?q=" + query + "&take=" + take + "&skip=" + skip
        );
    }
}