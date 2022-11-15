namespace I2R.LightNews.Models;

public class NewsSource
{
    public string Name { get; set; }
    public string CanonicalUrl { get; set; }
    public string Attribution { get; set; }
    public DateTime Created { get; set; }
    public List<NewsArticle> Articles { get; set; }
}