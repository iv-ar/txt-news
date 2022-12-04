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
        var letters = "#ABCDEFGHIJKLMNOPQRSTUVWXYZÆØÅ".ToCharArray();
        var skip = 0;
        foreach (var letter in letters) {
            
            // TODO: Support series._links.podcast in addition to series._links.seasons
            var path = "/radio/search/categories/alt-innhold?letter=" + letter == "#" ? "%23" : letter + "&skip=0&take=50";
            while (path.HasValue()) {
                var response = await _http.GetFromJsonAsync<RadioCategorySearchResult>(path);
                if (response == default) break;
                await Task.Delay(2000);
                foreach (var series in response.Series) {
                    var dbSeries = RadioIndexDb.GetSeriesByNrkId(series.Id) ?? new RadioSeries {
                        Name = series.Title,
                        NrkId = series.Id,
                        Type = series.Type,
                    };
                    var seriesId = dbSeries.Id > 0 ? dbSeries.Id : RadioIndexDb.AddSeries(dbSeries);
                    _logger.LogInformation("Added series {0} with id {1}, to the database", dbSeries.Name, seriesId);
                    if (!series.Links?.Series?.Href?.HasValue() ?? true) continue;
                    var seriesMetadata = await _http.GetFromJsonAsync<NrkRadioSeries>(series.Links.Series.Href);
                    if (seriesMetadata == default) continue;
                    await Task.Delay(1000);
                    if (seriesMetadata.Embedded.Seasons?.Any() ?? false) {
                        foreach (var season in seriesMetadata.Embedded.Seasons) {
                            var dbSeason = RadioIndexDb.GetSeasonByNrkId(season.Id) ?? new RadioSeason() {
                                Name = season.Titles.Title,
                                NrkId = season.Id,
                                SeriesId = seriesId
                            };
                            var seasonId = dbSeason.Id > 0 ? dbSeason.Id : RadioIndexDb.AddSeason(dbSeason);
                            _logger.LogInformation("Added season {0} to series {1} with id {2}, to the database", dbSeason.Name, dbSeries.Name, seasonId);
                            await AddEpisodesAsync(season.Episodes.Embedded.Episodes, dbSeries, dbSeason);
                        }
                    } else if (seriesMetadata.Embedded.Episodes.Embedded.Episodes?.Any() ?? false) {
                        await AddEpisodesAsync(seriesMetadata.Embedded.Episodes.Embedded.Episodes, dbSeries);
                    }
                }

                path = response.Links.NextPage.Href.HasValue() ? response.Links.NextPage.Href : "";
            }
        }
    }

    private async Task AddEpisodesAsync(List<NrkRadioSeries.EmbeddedModel.SeasonModel.EpisodeModel.EmbeddedModel.EpisodeModel> lol, RadioSeries dbSeries, RadioSeason dbSeason = default) {
        foreach (var episode in lol) {
            var dbEpisode = RadioIndexDb.GetEpisodeByNrkId(episode.EpisodeId) ?? new RadioEpisode {
                CanonicalUrl = episode.Links.Share.Href,
                Name = episode.Titles.Title,
                Description = episode.Titles.Subtitle,
                NrkId = episode.EpisodeId,
                SeasonId = dbSeason?.Id ?? -1,
                SeriesId = dbSeries.Id
            };

            if (dbEpisode.NrkId.IsNullOrWhiteSpace()) {
                _logger.LogWarning("No nrk id was found for episode {0}", dbEpisode.Name);
                continue;
            }

            var playbackResponse = await _http.GetFromJsonAsync<NrkPlaybackManifest>("/playback/manifest/program/" + dbEpisode.NrkId);
            if (playbackResponse == default) continue;
            dbEpisode.SourceUrl = playbackResponse.Playable.Assets.FirstOrDefault()?.Url;
            var episodeId = dbEpisode.Id > 0 ? dbEpisode.Id : RadioIndexDb.AddEpisode(dbEpisode);
            _logger.LogInformation("Added episode {0} to series {1} season {2} with id {3}, to the database", dbEpisode.Name, dbSeries.Name, dbSeason?.Name ?? "!!NO SEASON!!", episodeId);
        }
    }

    public async Task<RadioCategorySearchResult> SearchCategoriesAsync(string query, int take = 50, int skip = 0) {
        return await _http.GetFromJsonAsync<RadioCategorySearchResult>(
            "/radio/search/categories/alt-innhold?q=" + query + "&take=" + take + "&skip=" + skip
        );
    }
}