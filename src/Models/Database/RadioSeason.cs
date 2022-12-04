namespace I2R.LightNews.Models;

public class RadioSeason
{
    public int Id { get; set; }
    public int SeriesId { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string CanonicalUrl { get; set; }
    public string NrkId { get; set; }
}