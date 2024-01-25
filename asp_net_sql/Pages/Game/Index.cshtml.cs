
//
using asp_net_sql.Data;
using asp_net_sql.GameEngine;
using asp_net_sql.Models;

//
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace asp_net_sql.Pages.CodeBehind;

public class ChoosePlayer_CB(TicTacToe_Context _dbContext) : PageModel
{
    readonly TicTacToe_Context dbContext = _dbContext;

    readonly DbSet<EnumGameRoster> DbSet = _dbContext.EnumGameRosters;

    public List<EnumGameRoster> AsyncDbSetItems = [];

    public async Task OnGetAsync()
    {
        AsyncDbSetItems = await DbSet.ToListAsync();

        Engine.SetRoster(AsyncDbSetItems);
    }
}
