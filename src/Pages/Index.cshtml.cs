using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace I2R.LightNews.Pages;

public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;
    private readonly GrabberService _grabber;

    public IndexModel(ILogger<IndexModel> logger, GrabberService grabber) {
        _logger = logger;
        _grabber = grabber;
    }

    public NewsSource Source { get; set; }

    public async Task<ActionResult> OnGet(string site) {
        Source = site switch {
            "nrk" => await _grabber.GrabNrkAsync(),
            _ => default
        };

        if (Source == default) {
            return Redirect("/nrk");
        }

        return Page();
    }
}