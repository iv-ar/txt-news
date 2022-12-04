namespace I2R.LightNews.Models;

public class NrkLinks
{
    public LinkModel NextPage { get; set; }
    public LinkModel LastPage { get; set; }
    public LinkModel Share { get; set; }
    public LinkModel Episodes { get; set; }

    public class LinkModel
    {
        public string Href { get; set; }
    }
}