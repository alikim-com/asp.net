using asp_net_sql.Data;
using asp_net_sql.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using static asp_net_sql.Pages.Result;

namespace asp_net_sql.Pages;

public class Result
{
    public enum ResType
    {
        None,
        OK,
        Error
    }
    public ResType type = ResType.None;
    public Dictionary<string, string[]> info = [];
}

public class Admin_IndexModel(TicTacToe_Context dbContext) : PageModel
{
    private readonly TicTacToe_Context _dbContext = dbContext;

    public List<EnumGameRoster> EnumGameRosterItems { get; set; } = [];

    public async Task OnGetAsync()
    {
        EnumGameRosterItems = await _dbContext.EnumGameRosters.ToListAsync();
    }
    public async Task<IActionResult> OnPostChangeOriginAsync(int pkey, string identVal)
    {
        var result = new Result();

        var item = await _dbContext.EnumGameRosters.FindAsync(pkey);
        if (item != null)
        {
            if (!ModelState.IsValid) {

                foreach(var key in ModelState.Keys)
                {
                    var value = ModelState[key];
                    if(value != null && value.ValidationState == Microsoft.AspNetCore.Mvc.ModelBinding.ModelValidationState.Invalid)
                    {
                        var errors = value.Errors.Select(ent => ent.ErrorMessage).ToArray();
                        result.info.Add(key, errors);
                    }
                }
                result.type = ResType.Error;
                ViewData["Result"] = result;
                return Page();
            }

            item.Identity = identVal;
            try
            {
                int cnt = await _dbContext.SaveChangesAsync();
                result.type = ResType.OK;
                result.info.Add(
                    "Admin_IndexModel.SaveChangesAsync", 
                    [$"Success, {cnt} row affected"]);
            }
            catch (Exception ex)
            {
                result.type = ResType.Error;
                result.info.Add("Admin_IndexModel.SaveChangesAsync.Exception", [ex.Message]);
            }

            ViewData["Result"] = result;
            return Page();

        }

        return RedirectToPage();
    }
}
