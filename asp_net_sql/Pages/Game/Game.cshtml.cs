
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
using System.Collections.Generic;

namespace asp_net_sql.Pages.CodeBehind;

public class Game_CB(TicTacToe_Context _dbContext) : PageModel
{
    readonly TicTacToe_Context dbContext = _dbContext;

    public async Task OnGetAsync()
    {
        await Task.Delay(0);
    }

    PageResult PageWithData(
        Result result,
        HttpRequest req)
    {
        string baseURI = req.Scheme + "://" + req.Host.ToUriComponent();

        ViewData["Result"] = result;
        ViewData["baseURI"] = baseURI;

        return Page();
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
            return PageWithData(
                new Result(ResType.Error, caller, "id(s) not found"),
                Request);

        if (chosenLeft == chosenRight)
            return PageWithData(
                new Result(ResType.Error, caller, "bad ids"),
                Request);

        await Task.Delay(0);

        return PageWithData(
            new Result(ResType.OK, caller, "ready"),
            Request);
    }
}