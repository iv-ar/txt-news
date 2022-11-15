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
        if (site.IsNullOrWhiteSpace()) {
            return Redirect("/nrk");
        }

        Source = site switch {
            "nrk" => await _grabber.GrabNrkAsync(),
            _ => await _grabber.GrabNrkAsync()
        };

        return Page();
    }
}