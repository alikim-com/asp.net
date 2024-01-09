using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;

namespace asp_net_sql.Pages.CodeBehind;

public class PostData(Dictionary<string, string> _dict)
{
    public Dictionary<string, string> Dict { get; set; } = _dict;
}

public class API_Post_Model : PageModel
{
    static string json = "";

    static async Task<HttpResponseMessage> SendPostRequestAsync()
    {
        string apiEndpoint = "https://httpbin.org/anything";

        using HttpClient client = new();

        var postData = new PostData(new() {
            { "key1", "value1" },
            { "key2", "value2" } 
        });

        json = JsonSerializer.Serialize(postData);

        HttpContent content = new StringContent(
            json, 
            System.Text.Encoding.UTF8, 
            "application/json");

       return await client.PostAsync(apiEndpoint, content);

    }

    public async Task OnGetAsync()
    {
        HttpResponseMessage response = await SendPostRequestAsync();

        ViewData["json"] = json;

        if (response.IsSuccessStatusCode)
        {
            string responseBody = await response.Content.ReadAsStringAsync();

            ViewData["responseBody"] = responseBody;

        } else
        {
            ViewData["StatusCode"] = response.StatusCode;
        }
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await Task.Delay(0);

        return RedirectToPage();
    }

}
