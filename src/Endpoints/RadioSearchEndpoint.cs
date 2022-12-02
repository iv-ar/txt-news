using Microsoft.AspNetCore.Mvc;

namespace I2R.LightNews.Endpoints;

public class RadioSearchEndpoint : EndpointBase
{
    private readonly NrkRadioService _radio;

    public RadioSearchEndpoint(NrkRadioService radio) {
        _radio = radio;
    }

    [HttpGet("~/radio-search")]
    public async Task HandleASync(string q) {
        
    }
}