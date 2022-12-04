using Microsoft.AspNetCore.Mvc;

namespace I2R.LightNews.Endpoints;

public class RadioSearchEndpoint : EndpointBase
{
    private readonly NrkRadioService _radio;

    public RadioSearchEndpoint(NrkRadioService radio) {
        _radio = radio;
    }

    [HttpGet("~/create-radio-index")]
    public async Task<ActionResult> HandleASync() {
        await _radio.CreateIndex();
        return Ok();
    }
}