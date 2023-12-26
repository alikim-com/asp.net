using asp_net_sql.Data;
using asp_net_sql.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace asp_net_sql.Pages;

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
        var item = await _dbContext.EnumGameRosters.FindAsync(pkey);
        if (item != null)
        {
            item.Identity = identVal;
            try
            {
                await _dbContext.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                ModelState.AddModelError("Admin_IndexModel.OnPostChangeOriginAsync.DbUpdateException", ex.Message);
                return Page();
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("Admin_IndexModel.OnPostChangeOriginAsync.Exception", ex.Message);
                return Page();
            }
        }

        return RedirectToPage();
    }
}
