using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;
//
using asp_net_sql.Common;

namespace asp_net_sql.Pages.CodeBehind;

public class APIPostTest_CB : PageModel
{
    public async Task OnGetAsync()
    {
        Post.Context(
            Request,
            "/API/Index",
            new APIPacket(new() {
                { "key1", "value1" },
                { "key2", "value2" }
            },
            "status",
            "message"
            ),
            out string apiEndpoint,
            out HttpClient client,
            out HttpContent content,
            out string json);

        HttpResponseMessage response = await 
            client.PostAsync(apiEndpoint, content);

        ViewData["json"] = json;

        if (response.IsSuccessStatusCode)
        {
            string responseBody = await 
                response.Content.ReadAsStringAsync();

            // FINISH PARSING AND PRINT
            // //(APIPacket and Result -> .ToString)

            ViewData["responseBody"] = responseBody;

        }
        
        ViewData["StatusCode"] = response.StatusCode;

        ViewData["ReasonPhrase"] = response.ReasonPhrase;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await Task.Delay(0);

        return RedirectToPage();
    }

}
