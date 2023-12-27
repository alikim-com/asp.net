using asp_net_sql.Data;
using asp_net_sql.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Collections;
using System.Collections.Generic;
using static asp_net_sql.Pages.Result;

namespace asp_net_sql.Pages;

public static class EntityHelper
{
    public static string[] GetColumnNames<T>() where T : class
    {
        var propertyNames = typeof(T).GetProperties()
            .Where(property => IsSimpleType(property.PropertyType))
            .Select(property => property.Name)
            .ToArray();

        return propertyNames;
    }

    private static bool IsSimpleType(Type type)
    {
        return type.IsPrimitive || type.IsValueType || type == typeof(string);
    }
}

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

public class Admin_IndexModel<T> : PageModel where T : class
{
    private readonly TicTacToe_Context _dbContext;

    readonly DbSet<T> DbSet;
    public List<T> DbSetItems = [];

    public Admin_IndexModel(TicTacToe_Context dbContext)
    {
        _dbContext = dbContext;

        var dict = _dbContext.GetDbSetDictionary();

        if (!dict.TryGetValue(typeof(T), out var obj)) 
            throw new Exception($"Admin_IndexModel.GetDbSetDictionary : type '{typeof(T)}' not found");
       
        DbSet = (DbSet<T>)obj!;

       var strArr = EntityHelper.GetColumnNames<T>();
       // ViewData["Columns"] = strArr;
    }

    public async Task OnGetAsync()
    {
        DbSetItems = await DbSet.ToListAsync();
    }

    public async Task<PageResult> PageWithResult(Result result, ResType type)
    {
        result.type = type;
        ViewData["Result"] = result;
        await OnGetAsync();
        return Page();
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
                return await PageWithResult(result, ResType.Error);
            }

            item.Identity = identVal;

            ResType resType;

            try
            {
                int cnt = await _dbContext.SaveChangesAsync();
                resType = ResType.OK;
                result.info.Add(
                    "Admin_IndexModel.SaveChangesAsync", 
                    [$"Success, {cnt} row affected"]);
            }
            catch (Exception ex)
            {
                resType = ResType.Error;
                result.info.Add("Admin_IndexModel.SaveChangesAsync.Exception", [ex.Message]);
            }

            return await PageWithResult(result, resType);

        }

        return RedirectToPage();
    }
}
