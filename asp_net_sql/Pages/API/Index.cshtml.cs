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

        var resp = new Packet();

        try
        {
            var postData = JsonSerializer.Deserialize<Packet>
                (reqBody, Post.includeFields) ?? throw new Exception
                    ($"API.OnPostAsync : postData is null");

            switch(postData.command)
            {
                case PackCmd.Test:
                    resp.status = PackStat.Success;
                    resp.message = "received packet";
                    resp.command = postData.command;
                    resp.keyValuePairs = postData.keyValuePairs;
                    break;

                default:
                    break;
            }

            
        }
        catch (Exception ex)
        {
            resp.status = PackStat.Fail;
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
