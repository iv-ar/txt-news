using Microsoft.AspNetCore.Mvc;

namespace I2R.LightNews.Endpoints;

public class RadioSearchEndpoint : EndpointBase
{
    public RadioSearchEndpoint() { }

    public class Response
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    [HttpGet("~/radio-series")]
    public async Task<ActionResult<List<Response>>> HandleAsync(string q) {
        var series = RadioIndexDb.GetSeries(q);
        return Ok(series.Select(c => new Response() {
            Id = c.Id,
            Name = c.Name
        }));
    }
}