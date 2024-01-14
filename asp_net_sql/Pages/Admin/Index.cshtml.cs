using Microsoft.EntityFrameworkCore;
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
    MethodInfo? OnPostCRUDAsyncGen;

    // feedback data from gen class for RazorPage

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

        OnPostCRUDAsyncGen = Admin_CB_GenType.GetMethod("OnPostCRUDAsync")
            ?? throw new Exception
           ($"Admin_CB.Ctor : OnPostCRUDAsync<{DbSetTEntityName}> is null");
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

    public async Task<IActionResult> OnPostCRUDAsync()
    {
        if (string.IsNullOrWhiteSpace(DbSetTEntityName))
            return Page();

        DeferredCtor();

        return await (dynamic)OnPostCRUDAsyncGen!.Invoke(
            Admin_CB_GenInstance,
            [
                HttpContext.Request,
                ModelState
            ])!;
    }
}

public class Admin_CB<T> : PageModel where T : class, new()
{
    readonly TicTacToe_Context dbContext;
    /// <summary>
    /// Feedback for RazorPage
    /// </summary>
    readonly CustomViewData ViewDataParent = new();

    /// <summary>
    /// DB Table
    /// </summary>
    readonly DbSet<T> DbSet;
    /// <summary>
    /// DB Table columns, i.e. DbSet<T> class props w/o nav ones
    /// </summary>
    readonly List<PropertyInfo> DbSetPropInfo;
    /// <summary>
    /// Table primary keys
    /// </summary>
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
        List<PropertyInfo> DbSetPropInfo)
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

        SetParentView(null, DbSetPropInfo);
    }

    /// <summary>
    /// Runs after OnPost, followed by RazorPage
    /// </summary>
    async Task<PageResult> PageWithResult(Result result)
    {
        // update after CRUD and before RazorPage
        AsyncDbSetItems = await DbSet.ToListAsync();

        SetParentView(result, DbSetPropInfo);

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
    public async Task<IActionResult> OnPostCRUDAsync(
        HttpRequest Request,
        ModelStateDictionary ModelState
        )
    {
        var result = new Result();

        if (!ModelState.IsValid)
        {
            result.type = ResType.Error;
            foreach (var key in ModelState.Keys)
            {
                var value = ModelState[key];
                if (value != null &&
                    value.ValidationState == ModelValidationState.Invalid)
                {
                    var errors = value.Errors.Select(ent => ent.ErrorMessage).ToList();
                    result.info.Add
                        ($"Admin_CB.OnPostAsync : Invalid model [{key}]", errors);
                }
            }
            return await PageWithResult(result);
        }

        var formData = Request.Form.ToDictionary();

        if (!Enum.TryParse(
            formData["crudtype"].ToString().Capitalise(),
            out CrudType ct)
            ) throw new Exception($"Admin_CB.OnPostAsync : form[crudtype] parse error");

        if (!Enum.TryParse(
            formData["crudaction"].ToString().Capitalise(),
            out CrudAction ca)
            ) throw new Exception($"Admin_CB.OnPostAsync : form[crudaction] parse error");

        var crud = new CRUD<T>(dbContext, DbSet, DbSetPKeys);

        switch (ca)
        {
            case CrudAction.Delete:
                var oldPropsD = FormToTypedProps(formData, "old__");
                result = await crud.Do(ct, ca, oldPropsD, []);
                return await PageWithResult(result);

            case CrudAction.Create:
                var newPropsC = FormToTypedProps(formData, "");
                result = await crud.Do(ct, ca, [], newPropsC);
                return await PageWithResult(result);

            case CrudAction.Update:
                var oldPropsU = FormToTypedProps(formData, "old__");
                var newPropsU = FormToTypedProps(formData, "");
                result = await crud.Do(ct, ca, oldPropsU, newPropsU);
                return await PageWithResult(result);

            default:
                return RedirectToPage();
        }
    }
}
