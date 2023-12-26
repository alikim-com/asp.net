﻿using asp_net_sql.Data;
using asp_net_sql.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace asp_net_sql.Pages;

public class IndexModel(TicTacToe_Context dbContext) : PageModel
{
    private readonly TicTacToe_Context _dbContext = dbContext;

    public List<EnumGameRoster> EnumGameRosterItems { get; set; } = [];

    public async Task OnGetAsync()
    {
        EnumGameRosterItems = await _dbContext.EnumGameRosters.ToListAsync();
    }
    public async Task<IActionResult> OnPostChangeOriginAsync(int pkey)
    {
        var item = await _dbContext.EnumGameRosters.FindAsync(pkey);
        if (item != null)
        {
            item.Identity = "New Identity Value";
            await _dbContext.SaveChangesAsync();
        }

        return RedirectToPage();
    }
}