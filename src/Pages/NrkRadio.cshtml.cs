using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace I2R.LightNews.Pages;

public class NrkRadio : PageModel
{
    public string FrontPageDataJSON { get; set; }

    public ActionResult OnGet() {
        return Page();
    }
}