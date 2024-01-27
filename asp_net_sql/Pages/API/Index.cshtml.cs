using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;
//
using asp_net_sql.Common;

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
        using var reader = new StreamReader(Request.Body);

        string reqBody = await reader.ReadToEndAsync();

        var resp = new APIPacket();

        try
        {
            var postData = JsonSerializer.Deserialize<APIPacket>
                (reqBody, Post.includeFields);
            resp.status = "success";
            resp.message = "";
            // test
            resp.keyValuePairs = postData?.keyValuePairs;
        }
        catch (Exception ex)
        {
            resp.status = "error";
            resp.message = "exception";
            resp.info["info"] = [];
            resp.AddExeptionInfo("info", ex);
        }

        return new JsonResult(resp, Post.includeFields)
        {
            StatusCode = 200
        };
    }
}
