using Microsoft.Extensions.Caching.Memory;

namespace I2R.LightNews.Services;

public class NrkRadioService
{
    private readonly IMemoryCache _cache;
    private readonly HttpClient _http;
    private const string CATEGORY_SEARCH_CACHE_KEY = "category_search";

    public NrkRadioService(IMemoryCache cache, HttpClient http) {
        _cache = cache;
        http.BaseAddress = new Uri("https://psapi.nrk.no");
        _http = http;
    }

    public async Task GetEverythingAsync() {
        var path = "/radio/search/categories/alt-innhold";
        var everything = new List<RadioSeries>();
        while (path.HasValue()) {
            var response = await _http.GetFromJsonAsync<RadioCategorySearchResult>(path);
            
        }
    }

    public async Task<RadioCategorySearchResult> SearchCategoriesAsync(string query, int take = 50, int skip = 50) {
        return await _http.GetFromJsonAsync<RadioCategorySearchResult>(
            "/radio/search/categories/alt-innhold?q=" + query + "&take=" + take + "&skip=" + skip
        );
    }
}