namespace I2R.LightNews.Models;

public class NrkPlaybackManifest
{
    public PlayableModel Playable { get; set; }

    public class PlayableModel
    {
        public List<Asset> Assets { get; set; }

        public class Asset
        {
            public string Url { get; set; }
            public string Format { get; set; }
            public string MimeType { get; set; }
            public bool Encrypted { get; set; }
        }
    }
}