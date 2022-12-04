namespace I2R.LightNews.Models;

public class RadioEpisode
{
    public int Id { get; set; }
    public int SeriesId { get; set; }
    public int SeasonId { get; set; }
    public string NrkId { get; set; }
    public string Title { get; set; }
    public string Subtitle { get; set; }
    public string SourceUrl { get; set; }
    public string CanonicalUrl { get; set; }
}