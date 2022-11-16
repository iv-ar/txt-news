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

    public NewsSource FrontPage { get; set; }
    public NewsArticle Article { get; set; }
    public string PageTitle { get; set; }

    public async Task<ActionResult> OnGet([FromRoute] string site, [FromQuery] string url = default) {
        PageTitle = site switch {
            "nrk" => "NRK",
            _ => ""
        };

        if (url.IsNullOrWhiteSpace()) {
            FrontPage = site switch {
                "nrk" => await _grabber.GrabNrkAsync(),
                _ => default
            };

            if (FrontPage == default) {
                return Redirect("/nrk");
            }
        } else {
            Article = site switch {
                "nrk" => await _grabber.GrabNrkArticleAsync(url),
                _ => default
            };

            if (Article == default) {
                return Redirect(url);
            }

            PageTitle = PageTitle + " - " + Article.Title;
        }

        return Page();
    }
}