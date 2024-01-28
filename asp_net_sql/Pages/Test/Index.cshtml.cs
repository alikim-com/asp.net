using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
//
using System.Diagnostics;
//
using asp_net_sql.Common;
using System.Text.Json;

namespace asp_net_sql.Pages.CodeBehind;

public class APIPostTest_CB : PageModel
{
    public async Task OnGetAsync()
    {
        var packet = new APIPacket(
            new() {
                { "key1", "value1" },
                { "key2", "value2" }
            },
            "status",
            "message",
            APICmd.StartEngine
            );

        //Debug.WriteLine(packet);

        Post.Context(
            Request,
            "/API/Index",
            packet,
            out string apiEndpoint,
            out HttpClient client,
            out HttpContent content,
            out string json);

        ViewData["Sent json"] = json;
        ViewData["Sent packet"] = packet.ToString();

        HttpResponseMessage response = await 
            client.PostAsync(apiEndpoint, content);

        ViewData["Response body"] = "";
        ViewData["Response packet"] = "";

        if (response.IsSuccessStatusCode)
        {
            string responseBody = await
            response.Content.ReadAsStringAsync();

            ViewData["Response body"] = responseBody;

            if (responseBody != null)
            {
                APIPacket? resp = JsonSerializer.Deserialize<APIPacket>
                    (responseBody, Post.includeFields);

                if(resp != null)
                ViewData["Response packet"] = resp.ToString();
            }
        }
        
        ViewData["Response status code"] = response.StatusCode;
        ViewData["Response reason phrase"] = response.ReasonPhrase;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await Task.Delay(0);

        return RedirectToPage();
    }

}
