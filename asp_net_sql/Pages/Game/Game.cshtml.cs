
//
using asp_net_sql.Data;
using asp_net_sql.Common;
using asp_net_sql.GameEngine;
using asp_net_sql.Models;
using Microsoft.AspNetCore.Mvc;

//
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Primitives;

namespace asp_net_sql.Pages.CodeBehind;

public class Game_CB(TicTacToe_Context _dbContext) : PageModel
{
    readonly TicTacToe_Context dbContext = _dbContext;

    public async Task OnGetAsync()
    {
        await Task.Delay(0);
    }
    public async Task<IActionResult> OnPostAsync()
    {
        var caller = "Game_CB.OnPostAsync";
        var formData = Request.Form.ToDictionary();
        if (!int.TryParse(
            Utils.SafeDictValue(formData, "chosenLeft", caller),
            out int chosenLeft)) throw new Exception($"{caller} : error parsing chosenLeft");
        if (!int.TryParse(
            Utils.SafeDictValue(formData, "chosenRight", caller),
            out int chosenRight)) throw new Exception($"{caller} : error parsing chosenRight");

        if (Engine.roster.FirstOrDefault
            (itm => itm.id == chosenLeft || itm.id == chosenRight) == null)
        {
            ViewData["Result"] = new Result(
                ResType.Error,
                new Dictionary<string, List<string>>() { { caller, ["id(s) not found"] } });
            return Page();
        }

        if (chosenLeft == chosenRight)
        {
            ViewData["Result"] = new Result(
                ResType.Error,
                new Dictionary<string, List<string>>() { { caller, ["bad ids"] } });
            return Page();
        }

        ViewData["Result"] = new Result(
            ResType.OK,
            new Dictionary<string, List<string>>() { { caller, ["ready"] } });

        // start engine in a diff thread - task.run()?
        // loading assets

        await Task.Delay(0);
        return Page();
    }
}