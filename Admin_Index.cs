using asp_net_sql.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using static asp_net_sql.Pages.Result;

using System.Reflection;
using asp_net_sql.Models;
using Microsoft.AspNetCore.Mvc.Razor;

namespace asp_net_sql.Pages;

public static class EntityHelper
{
    public static Type MakeGenericType(string genericType, string typeArgument)
    {
        Type? generic = Type.GetType(genericType) ?? throw new Exception
            ($"EntityHelper.MakeGenericType : generic type '{genericType}' not found");
        Type? argument = Type.GetType(typeArgument) ?? throw new Exception
            ($"EntityHelper.MakeGenericType : argument type '{typeArgument}' not found");

        return generic.MakeGenericType(argument);
    }

    public static MethodInfo MakeGenericMethod(Type T, string methodName, Type tArgument)
    {
        MethodInfo genericMethod = T.GetMethod(methodName) ?? throw new Exception
            ($"EntityHelper.MakeGenericMethod : no method {methodName} found for type {T.Name}");

        return genericMethod.MakeGenericMethod(tArgument);
    }

    public static Dictionary<string, Type> GetDbSetInfo(TicTacToe_Context dbContext) =>
        dbContext.GetType().GetProperties().Where(p =>
            p.PropertyType.IsGenericType &&
            p.PropertyType.GetGenericTypeDefinition() == typeof(DbSet<>))
            .Select(p => new KeyValuePair<string, Type>(
                p.PropertyType.GetGenericArguments()[0].Name,
                p.PropertyType.GetGenericArguments()[0]
                )).ToDictionary();

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

/// <summary>
/// Uses DeferredCtor to effectively implement
/// generic class version
/// Admin_IndexModel<DbSetTEntityName>
/// and generic model for the Razor page
/// @model Admin_IndexModel<DbSetTEntityName>
/// </summary>
public class Admin_IndexModel : PageModel
{
    readonly TicTacToe_Context dbContext;

    public readonly Dictionary<string, Type> DbSetInfo;

    public List<PropertyInfo> DbSetPropInfo = [];

    List<string> DbSetPKeys = [];

    object DbSet = new();

    MethodInfo? ToListAsyncGen;
    MethodInfo? FindAsyncGen;

    public object AsyncDbSetItems = new();

    [BindProperty(SupportsGet = true)]
    public string DbSetTEntityName { get; set; } = "";

    public Admin_IndexModel(TicTacToe_Context _dbContext)
    {
        dbContext = _dbContext;

        DbSetInfo = EntityHelper.GetDbSetInfo(dbContext);
    }

    public void DeferredCtor()
    {
        if (!DbSetInfo.TryGetValue(DbSetTEntityName, out Type? TEntity)) throw new Exception
            ($"Admin_IndexModel.DeferredCtor : no entity type found for {DbSetTEntityName}");

        var GetDbSetGen = EntityHelper.MakeGenericMethod(
            typeof(EntityHelper),
            "GetDbSet",
            TEntity);

        DbSet = GetDbSetGen.Invoke(null, [dbContext]) ?? throw new Exception
            ($"Admin_IndexModel.DeferredCtor : GetDbSet<{TEntity.Name}> invokation returned null");

        ToListAsyncGen = EntityHelper.MakeGenericMethod(
            typeof(EntityFrameworkQueryableExtensions),
            "ToListAsync",
            TEntity
            ) ?? throw new Exception
            ($"Admin_IndexModel.DeferredCtor : ToListAsyncGen<{TEntity.Name}> is null");

        // the one w/o cancellation token
        FindAsyncGen = DbSet.GetType().GetMethods().FirstOrDefault
            (m => m.Name == "FindAsync" && m.GetParameters().Length == 1);

        var GetClassPropInfoGen = EntityHelper.MakeGenericMethod(
            typeof(EntityHelper), 
            "GetClassPropInfo", 
            TEntity);

        object _DbSetPropInfo = GetClassPropInfoGen.Invoke(null, []) ?? throw new Exception
            ($"Admin_IndexModel.DeferredCtor : GetDbSet<{TEntity.Name}> invokation returned null");

        DbSetPropInfo = (List<PropertyInfo>)_DbSetPropInfo;

        var entityType = dbContext.Model.FindEntityType(TEntity);
        var pKey = (entityType?.FindPrimaryKey()) ??
            throw new Exception($"Admin_IndexModel.Ctor : pkey not found for type '{TEntity}'");

        DbSetPKeys = pKey.Properties.Select(p => p.Name).ToList();
    }

    Dictionary<string, object> TypedPropsFromQuery(IQueryCollection? qry, string suf) =>
        qry == null ? [] :
        DbSetPropInfo.Select(ent =>
            new KeyValuePair<string, object>(
                ent.Name,
                Convert.ChangeType(qry[ent.Name + suf].ToString(), ent.PropertyType)
            )).ToDictionary();

    // must pass params w/ def values
    async Task DbSetToListAsync() => AsyncDbSetItems = await (dynamic)ToListAsyncGen!.Invoke
            (null, new object[] { DbSet, default(CancellationToken) })!;

    public async Task OnGetAsync()
    {
        DeferredCtor();

        await DbSetToListAsync();
    }

    PageResult PageWithResult(Result result, ResType type)
    {
        result.type = type;
        ViewData["Result"] = result;

        return Page();
    }

    void UpdateDbSet(object item, Dictionary<string, object> props)
    {
        foreach (var pInf in DbSetPropInfo)
            pInf.SetValue(item, props[pInf.Name]);
    }

    public async Task<IActionResult> OnPostUpdateAsync()
    {
        DeferredCtor();

        var formData = Request.Form;

        var result = new Result();

        using var transaction = await dbContext.Database
            .BeginTransactionAsync(System.Data.IsolationLevel.RepeatableRead);
        try
        {
            var qry = HttpContext.Request.Query;

            var oldProps = TypedPropsFromQuery(qry, "_old");
            var newProps = TypedPropsFromQuery(qry, "_new");

            await DbSetToListAsync();

            object[] PKeyValues = DbSetPKeys.Select(pk => oldProps[pk]).ToArray();

            var item = await (dynamic)FindAsyncGen!.Invoke
                (DbSet, new object[] { PKeyValues })!;

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
