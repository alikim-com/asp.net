﻿using Microsoft.EntityFrameworkCore;
//
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Primitives;
//
using asp_net_sql.Data;
using asp_net_sql.Common;
//
using System.Reflection;

namespace asp_net_sql.Pages.CodeBehind;

public class CustomViewData()
{
    public Result? result = null;
    public List<PropertyInfo>? DbSetPropInfo = [];
    public List<dynamic>? AsyncDbSetItems = [];
}

public class Admin_CB(TicTacToe_Context _dbContext) : PageModel
{
    // primary ctor

    readonly TicTacToe_Context dbContext = _dbContext;
    public readonly Dictionary<string, Type> DbSetInfo = EntityHelper.GetDbSetInfo(_dbContext);

    // generic class

    [BindProperty(SupportsGet = true)]
    public string DbSetTEntityName { get; set; } = "";

    object Admin_CB_GenInstance = new() { };
    MethodInfo? OnGetAsyncGen;
    MethodInfo? OnPostUpdateAsyncGen;

    // data from gen class

    public readonly CustomViewData ViewDataGen = new();

    void DeferredCtor()
    {
        var Admin_CB_GenType = EntityHelper.MakeGenericType(
            typeof(Admin_CB<>),
            "asp_net_sql.Models." + DbSetTEntityName) ?? throw new Exception
            ($"Admin_CB.Ctor : failed to make gen type Admin_CB<{DbSetTEntityName}>");

        var Admin_CB_Gen = Activator.CreateInstance(
            Admin_CB_GenType,
            new object[] { dbContext, ViewDataGen }) ?? throw new Exception
            ($"Admin_CB.Ctor : failed to make instance of Admin_CB<{DbSetTEntityName}>");

        Admin_CB_GenInstance = Admin_CB_Gen;

        OnGetAsyncGen = Admin_CB_GenType.GetMethod("OnGetAsync")
            ?? throw new Exception
           ($"Admin_CB.Ctor : OnGetAsync<{DbSetTEntityName}> is null");

        OnPostUpdateAsyncGen = Admin_CB_GenType.GetMethod("OnPostUpdateAsync")
            ?? throw new Exception
           ($"Admin_CB.Ctor : OnPostUpdateAsync<{DbSetTEntityName}> is null");
    }

    public async Task OnGetAsync()
    {
        // don't run for /Admin
        if (string.IsNullOrWhiteSpace(DbSetTEntityName))
            return;

        DeferredCtor();

        await (dynamic)OnGetAsyncGen!.Invoke(
            Admin_CB_GenInstance,
            [])!;
    }

    public async Task<IActionResult> OnPostUpdateAsync()
    {
        if (string.IsNullOrWhiteSpace(DbSetTEntityName))
            return Page();

        DeferredCtor();

        return await (dynamic)OnPostUpdateAsyncGen!.Invoke(
            Admin_CB_GenInstance,
            [
                HttpContext.Request,
                ModelState
            ])!;
    }
}

public class Admin_CB<T> : PageModel where T : class
{
    readonly TicTacToe_Context dbContext;
    readonly CustomViewData ViewDataParent = new();

    readonly DbSet<T> DbSet;
    readonly List<PropertyInfo> DbSetPropInfo;
    readonly List<string> DbSetPKeys;

    public List<T> AsyncDbSetItems;

    public Admin_CB(
        TicTacToe_Context _dbContext,
        CustomViewData _ViewDataParent)
    {
        dbContext = _dbContext;
        ViewDataParent = _ViewDataParent;

        DbSet = EntityHelper.GetDbSet<T>(dbContext);

        DbSetPropInfo = EntityHelper.GetClassPropInfo<T>();

        var DbSetType = typeof(T);
        var entityType = dbContext.Model.FindEntityType(DbSetType);
        var pKey = (entityType?.FindPrimaryKey()) ??
            throw new Exception($"Admin_CB.Ctor : pkey not found for type '{DbSetType}'");

        DbSetPKeys = pKey.Properties.Select(p => p.Name).ToList();

        AsyncDbSetItems = [];
    }

    void SetParentView(
        Result? resultGen,
        List<PropertyInfo> DbSetPropInfo,
        List<T> AsyncDbSetItems)
    {
        ViewDataParent.result = resultGen;
        ViewDataParent.DbSetPropInfo = DbSetPropInfo;
        ViewDataParent.AsyncDbSetItems = AsyncDbSetItems.Select
            (item => (dynamic)item).ToList();
    }

    /// <summary>
    /// Runs after Ctor
    /// </summary>
    public async Task OnGetAsync()
    {
        AsyncDbSetItems = await DbSet.ToListAsync();

        SetParentView(null, DbSetPropInfo, AsyncDbSetItems);
    }

    /// <summary>
    /// Runs after OnPost, followed by RazorPage
    /// </summary>
    PageResult PageWithResult(Result result, ResType type)
    {
        result.type = type;

        SetParentView(result, DbSetPropInfo, AsyncDbSetItems);

        return Page();
    }

    void UpdateDbSet(T item, Dictionary<string, object> props)
    {
        foreach (var pInf in DbSetPropInfo)
            pInf.SetValue(item, props[pInf.Name]);
    }

    Dictionary<string, object> FormToTypedProps(
        Dictionary<string, StringValues> formData,
        string prf) =>
            formData == null ? [] :
            DbSetPropInfo.Select(ent =>
                new KeyValuePair<string, object>(
                    ent.Name,
                    Convert.ChangeType(formData[prf + ent.Name].ToString(), ent.PropertyType)
                )).ToDictionary();

    /// <summary>
    /// Runs after Ctor
    /// </summary>
    public async Task<IActionResult> OnPostUpdateAsync(
        HttpRequest Request,
        ModelStateDictionary ModelState
        )
    {
        var result = new Result();

        using var transaction = await dbContext.Database
            .BeginTransactionAsync(System.Data.IsolationLevel.RepeatableRead);
        try
        {
            var formData = Request.Form.ToDictionary();

            AsyncDbSetItems = await DbSet.ToListAsync();

            var newProps = FormToTypedProps(formData, "");
            var oldProps = FormToTypedProps(formData, "old__");

            object[] PKeyValues = DbSetPKeys.Select(pk => oldProps[pk]).ToArray();
            T? item = await DbSet.FindAsync(PKeyValues);

            if (item != null)
            {
                if (!ModelState.IsValid)
                {
                    foreach (var key in ModelState.Keys)
                    {
                        var value = ModelState[key];
                        if (value != null &&
                            value.ValidationState == ModelValidationState.Invalid)
                        {
                            var errors = value.Errors.Select(ent => ent.ErrorMessage).ToList();
                            result.info.Add("<Invalid model> " + key, errors);
                        }
                    }
                    return PageWithResult(result, ResType.Error);
                }

                UpdateDbSet(item, newProps);

                ResType resType;

                try
                {
                    int cnt = await dbContext.SaveChangesAsync();

                    await transaction.CommitAsync();

                    resType = ResType.OK;
                    result.info.Add(
                        "Admin_CB.SaveChangesAsync",
                        [$"Success, {cnt} row affected"]);
                }
                catch (Exception ex)
                {
                    UpdateDbSet(item, oldProps);

                    resType = ResType.Error;
                    result.info.Add(
                        "Admin_CB.SaveChangesAsync",
                        ["Exception: " + ex.Message]);
                }

                return PageWithResult(result, resType);
            }

        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();

            result.info.Add(
                "Admin_CB.OnPostUpdateAsync",
                ["Transaction exception: " + ex.Message]);
            return PageWithResult(result, ResType.Error);
        }

        return RedirectToPage();
    }
}
