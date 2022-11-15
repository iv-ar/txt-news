namespace I2R.LightNews.Models;

public class NewsArticle
{
    public string Title { get; set; }
    public string Subtitle { get; set; }
    public string Href { get; set; }
    public string Content { get; set; }
    public List<Author> Authors { get; set; }
    public DateTimeOffset CachedAt { get; set; }
    public DateTime PublishedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public class Author
    {
        public string Name { get; set; }
        public string Contact { get; set; }
        public string Title { get; set; }
    }
}