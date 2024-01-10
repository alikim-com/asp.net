using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;

namespace asp_net_sql.Pages.CodeBehind;

public class PostData(Dictionary<string, string> _dict)
{
    public Dictionary<string, string> Dict { get; set; } = _dict;
}

public class APIPostTest_CB : PageModel
{
    static void PreparePost(
        HttpRequest req,
        string url,
        PostData data,
        out string apiEndpoint,
        out HttpClient client,
        out HttpContent content,
        out string json)
    {
        string selfURI = req.Scheme + "://" + req.Host.ToUriComponent();

        apiEndpoint = selfURI + url;

        client = new();

        json = JsonSerializer.Serialize(data);

        content = new StringContent(
            json,
            System.Text.Encoding.UTF8,
            "application/json");
    }

    public async Task OnGetAsync()
    {
        PreparePost(
            Request,
            "/API/Index",
            new PostData(new() {
                { "key1", "value1" },
                { "key2", "value2" }
            }),
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
