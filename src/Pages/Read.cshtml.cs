using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace I2R.LightNews.Pages;

public class ReadModel : PageModel
{
    private readonly GrabberService _grabber;

    public NewsArticle Source { get; set; }

    public ReadModel(GrabberService grabber) {
        _grabber = grabber;
    }

    public async Task<ActionResult> OnGet([FromRoute] string site, [FromQuery] string url) {
        Source = site switch {
            "nrk" => await _grabber.GrabNrkArticleAsync(url),
            _ => default
        };
        if (Source == default) return Redirect(url);
        return Page();
    }
}