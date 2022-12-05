using Microsoft.AspNetCore.Mvc;

namespace I2R.LightNews.Endpoints;

public class RadioSearchEndpoint : EndpointBase
{
    public RadioSearchEndpoint() { }

    [HttpGet("~/radio-series")]
    public async Task<ActionResult> HandleAsync(string q) {
        return Ok(RadioIndexDb.GetSeries(q));
    }
}