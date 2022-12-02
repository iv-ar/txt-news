namespace I2R.LightNews.Models;

public class RadioSeries
{
    public string Name { get; set; }
    public string Description { get; set; }
    public string Type { get; set; }
    public Uri CanonicalUri { get; set; }
    public List<Episode> Episodes { get; set; }

    public class Episode
    {
        public string Title { get; set; }
        public string Subtitle { get; set; }
        public Uri SourceUri { get; set; }
        public Uri CanonicalUri { get; set; }
    }
}