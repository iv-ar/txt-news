using System.Text.Json.Serialization;

namespace I2R.LightNews.Models;

public class NrkRadioSeries
{
    [JsonPropertyName("_links")]
    public NrkLinks Links { get; set; }

    [JsonPropertyName("_embedded")]
    public EmbeddedModel Embedded { get; set; }

    public class EmbeddedModel
    {
        public List<SeasonModel> Seasons { get; set; }

        public class SeasonModel
        {
            public List<TitleModel> Titles { get; set; }
            public List<EpisodeModel> Episodes { get; set; }
            public string Id { get; set; }
            public bool HasAvailableEpisodes { get; set; }
            public int EpisodeCount { get; set; }

            public class EpisodeModel
            {
                [JsonPropertyName("_embedded")]
                public EmbeddedModel Embedded { get; set; }

                public class EmbeddedModel
                {
                    public List<EpisodeModel> Episodes { get; set; }

                    public class EpisodeModel
                    {
                        [JsonPropertyName("_links")]
                        public LinksModel Links { get; set; }

                        public string Id { get; set; }
                        public string EpisodeId { get; set; }
                        public List<TitlesModel> Titles { get; set; }
                        public DateTime Date { get; set; }
                        public int DurationInSeconds { get; set; }
                        public int ProductionYear { get; set; }

                        public class TitlesModel
                        {
                            public string Title { get; set; }
                            public string Subtitle { get; set; }
                        }

                        public class LinksModel
                        {
                            public NrkLinks.LinkModel Playback { get; set; }
                            public NrkLinks.LinkModel Share { get; set; }
                        }
                    }
                }
            }

            public class TitleModel
            {
                public string Title { get; set; }
            }
        }
    }

    public class NrkRadioSeriesLinks : NrkLinks
    {
        public List<Season> Seasons { get; set; }

        public class Season
        {
            public string Name { get; set; }
            public string Href { get; set; }
            public string Title { get; set; }
        }
    }
}