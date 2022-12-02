using System.Text.Json.Serialization;

namespace I2R.LightNews.Models;

public class RadioCategorySearchResult
{
    [JsonPropertyName("_links")]
    public LinksModel Links { get; set; }

    public List<LetterModel> Letters { get; set; }
    public string Title { get; set; }
    public List<SeriesModel> Series { get; set; }
    public long TotalCount { get; set; }

    public class SeriesModel
    {
        [JsonPropertyName("_links")]
        public LinksModel Links { get; set; }

        public Guid Id { get; set; }
        public string SeriesId { get; set; }
        public string SeasonId { get; set; }
        public string Title { get; set; }
        public string Type { get; set; }
        public string InitialCharacter { get; set; }
        public List<ImageModel> Images { get; set; }

        public class ImageModel
        {
            public string Uri { get; set; }
            public int Width { get; set; }
        }

        public class LinksModel
        {
            public RadioCategorySearchResult.LinksModel.LinkModel CustomSeason { get; set; }
        }
    }

    public class LetterModel
    {
        public string Letter { get; set; }
        public int Count { get; set; }
        public string Link { get; set; }
    }

    public class LinksModel
    {
        public LinkModel NextPage { get; set; }
        public LinkModel LastPage { get; set; }


        public class LinkModel
        {
            public string Href { get; set; }
        }
    }
}