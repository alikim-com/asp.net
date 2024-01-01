using asp_net_sql.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using static asp_net_sql.Pages.Result;

using System.Reflection;
using System.Diagnostics;

namespace asp_net_sql.Pages;

public static class EntityHelper
{
    public static List<string> GetDbSetNames(TicTacToe_Context dbContext) => 
        dbContext.GetType().GetProperties().Where(p =>
            p.PropertyType.IsGenericType &&
            p.PropertyType.GetGenericTypeDefinition() == typeof(DbSet<>))
            .Select(p => p.PropertyType.GetGenericArguments()[0].Name).ToList();

    public static DbSet<T> GetDbSet<T>(TicTacToe_Context dbContext) where T : class
    {
        var typePInf = dbContext.GetType().GetProperties()
        .FirstOrDefault(p => p.PropertyType == typeof(DbSet<T>));

        var dbSetGen = typePInf?.GetValue(dbContext);

        if (dbSetGen is not DbSet<T> dbSet)
            throw new Exception($"EntityHelper.GetDbSet : error getting DbSet<{nameof(T)}> from dbContext");

        return dbSet;
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
    readonly TicTacToe_Context dbContext;

    readonly List<string> DbSetNames;

    readonly DbSet<T> DbSet;
    readonly List<PropertyInfo> DbSetPropInfo;
    readonly List<string> DbSetPKeys;

    public List<T> AsyncDbSetItems;

    public Admin_IndexModel(TicTacToe_Context _dbContext)
    {
        dbContext = _dbContext;

        DbSetNames = EntityHelper.GetDbSetNames(dbContext);

        DbSet = EntityHelper.GetDbSet<T>(dbContext);

        DbSetPropInfo = EntityHelper.GetClassPropInfo<T>();

        var DbSetType = typeof(T);
        var entityType = dbContext.Model.FindEntityType(DbSetType);
        var pKey = (entityType?.FindPrimaryKey()) ?? 
            throw new Exception($"Admin_IndexModel.Ctor : pkey not found for type '{DbSetType}'");

        DbSetPKeys = pKey.Properties.Select(p => p.Name).ToList();

        AsyncDbSetItems = [];
    }

    Dictionary<string, object> TypedPropsFromQuery(IQueryCollection? qry, string suf) =>
        qry == null ? [] :
        DbSetPropInfo.Select(ent =>
            new KeyValuePair<string, object>(
                ent.Name,
                Convert.ChangeType(qry[ent.Name + suf].ToString(), ent.PropertyType)
            )).ToDictionary();

    void SetViewData()
    {
        ViewData["DbSetNames"] ??= DbSetNames;
        ViewData["DbSetPropInfo"] ??= DbSetPropInfo;
    }

    /// <summary>
    /// Runs after Ctor
    /// </summary>
    public async Task OnGetAsync()
    {
        SetViewData();
        AsyncDbSetItems = await DbSet.ToListAsync();
    }

    /// <summary>
    /// Runs after OnPost, followed by RazorPage
    /// </summary>
    PageResult PageWithResult(Result result, ResType type)
    {
        result.type = type;
        ViewData["Result"] = result;
        SetViewData();

        return Page();
    }

    void UpdateDbSet(T item, Dictionary<string, object> props)
    {
        foreach (var pInf in DbSetPropInfo)
            pInf.SetValue(item, props[pInf.Name]);
    }

    /// <summary>
    /// Runs after Ctor
    /// </summary>
    public async Task<IActionResult> OnPostUpdateAsync()
    {
        var result = new Result();

        using var transaction = await dbContext.Database
            .BeginTransactionAsync(System.Data.IsolationLevel.RepeatableRead);
        try
        {
            var qry = HttpContext.Request.Query;

            var oldProps = TypedPropsFromQuery(qry, "_old");
            var newProps = TypedPropsFromQuery(qry, "_new");

            AsyncDbSetItems = await DbSet.ToListAsync();

            object[] PKeyValues = DbSetPKeys.Select(pk => oldProps[pk]).ToArray();
            var item = await DbSet.FindAsync(PKeyValues);

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
                        "Admin_IndexModel.SaveChangesAsync",
                        [$"Success, {cnt} row affected"]);
                }
                catch (Exception ex)
                {
                    UpdateDbSet(item, oldProps);

                    resType = ResType.Error;
                    result.info.Add(
                        "Admin_IndexModel.SaveChangesAsync", 
                        ["Exception: " + ex.Message]);
                }

                return PageWithResult(result, resType);
            }

        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();

            result.info.Add(
                "Admin_IndexModel.OnPostUpdateAsync", 
                ["Transaction exception: " + ex.Message]);
            return PageWithResult(result, ResType.Error);
        }

        return RedirectToPage();
    }
}
