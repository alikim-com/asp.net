using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace asp_net_sql.Pages.CodeBehind;

public class Index_Model() : PageModel
{
    public async Task OnGetAsync()
    {
        await Task.Delay(0);
    }
    public async Task<IActionResult> OnPostAsync()
    {
        await Task.Delay(0);

        return RedirectToPage();
    }
}
