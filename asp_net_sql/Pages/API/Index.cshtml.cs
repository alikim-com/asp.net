using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace asp_net_sql.Pages.CodeBehind;

[IgnoreAntiforgeryToken]
public class API_CB : PageModel
{
    public async Task OnGetAsync()
    {
        await Task.Delay(0);
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await Task.Delay(0);

        var responseData = new { 
            Message = "Operation successful" 
        };
        return new OkObjectResult(responseData)
        {
            StatusCode = 201
        };

        // var responseData = new { Message = "Operation successful" };
        // return new JsonResult(responseData);
    }

}
