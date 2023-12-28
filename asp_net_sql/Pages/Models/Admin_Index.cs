﻿using asp_net_sql.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using static asp_net_sql.Pages.Result;

using System.Reflection;
using asp_net_sql.Models;

namespace asp_net_sql.Pages;

public static class EntityHelper
{
    public static Dictionary<Type, object?> GetDbSetsInfo(TicTacToe_Context dbContext)
    {
        var dbSetProperties = dbContext.GetType().GetProperties()
            .Where(p => p.PropertyType.IsGenericType &&
                        p.PropertyType.GetGenericTypeDefinition() == typeof(DbSet<>));

        var infoDict = dbSetProperties.ToDictionary(
            p => p.PropertyType.GetGenericArguments()[0],
            p => p.GetValue(dbContext)
        );

        return infoDict;
    }

    public static List<PropertyInfo> GetClassPropInfo<T>() where T : class
    {
        var infoDict = typeof(T).GetProperties()
            .Where(property => IsSimpleType(property.PropertyType)).ToList();

        return infoDict;
    }

    private static bool IsSimpleType(Type type) =>
        type.IsPrimitive || type.IsValueType || type == typeof(string);
}

public class Result(
    ResType _type = ResType.None,
    Dictionary<string, string[]>? _info = null)
{
    public enum ResType
    {
        None,
        OK,
        Error
    }
    public ResType type = _type;
    public Dictionary<string, string[]> info = _info ?? [];
}

public class Admin_IndexModel<T> : PageModel where T : class
{
    private readonly TicTacToe_Context dbContext;

    readonly List<string> DbSetNames;

    readonly DbSet<T> DbSet;

    readonly List<PropertyInfo> DbSetPropInfo;
    public List<T> AsyncDbSetItems = [];

    public Admin_IndexModel(TicTacToe_Context _dbContext)
    {
        dbContext = _dbContext;

        var dict = EntityHelper.GetDbSetsInfo(dbContext);

        DbSetNames = dict.Keys.Select(k => k.Name).ToList();

        if (!dict.TryGetValue(typeof(T), out var obj))
            throw new Exception($"Admin_IndexModel.GetDbSetDictionary : type '{typeof(T)}' not found");

        DbSet = (DbSet<T>)obj!;

        DbSetPropInfo = EntityHelper.GetClassPropInfo<T>();

        var entityType = dbContext.Model.FindEntityType(typeof(T));
        var primaryKey = entityType?.FindPrimaryKey()?.Properties.Select(p => p.Name);
        // entityType?.FindPrimaryKey()?.Properties[0].Name
        if (primaryKey != null)
        {
        }
    }

    public async Task OnGetAsync()
    {
        ViewData["DbSetNames"] ??= DbSetNames;
        ViewData["DbSetPropInfo"] ??= DbSetPropInfo;

        AsyncDbSetItems = await DbSet.ToListAsync();
    }

    public async Task<PageResult> PageWithResult(Result result, ResType type)
    {
        result.type = type;
        ViewData["Result"] = result;
        await OnGetAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostChangeOriginAsync()
    {
        var qry = HttpContext.Request.Query;
        

        var result = new Result();

        var item = await DbSet.FindAsync(1);
        if (item != null)
        {
            if (!ModelState.IsValid)
            {

                foreach (var key in ModelState.Keys)
                {
                    var value = ModelState[key];
                    if (value != null && value.ValidationState == Microsoft.AspNetCore.Mvc.ModelBinding.ModelValidationState.Invalid)
                    {
                        var errors = value.Errors.Select(ent => ent.ErrorMessage).ToArray();
                        result.info.Add(key, errors);
                    }
                }
                return await PageWithResult(result, ResType.Error);
            }

           // item.Identity = identVal;

            ResType resType;

            try
            {
                int cnt = await dbContext.SaveChangesAsync();
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
