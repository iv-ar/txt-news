using Microsoft.AspNetCore.Mvc;

namespace I2R.LightNews.Endpoints;

public class GetRadioSeriesDetails : EndpointBase
{
    private readonly NrkRadioService _radio;

    public GetRadioSeriesDetails(NrkRadioService radio) {
        _radio = radio;
    }

    public class Response
    {
        public List<RadioSeason> Seasons { get; set; }
        public List<RadioEpisode> Episodes { get; set; }
    }

    [HttpGet("~/radio-series/{id:int}")]
    public ActionResult GetRadioSeries(int id) {
        var series = RadioIndexDb.GetSeriesById(id);
        if (series == default) {
            return NoContent();
        }
    }
}