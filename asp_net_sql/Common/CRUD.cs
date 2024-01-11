using asp_net_sql.Data;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace asp_net_sql.Common;

public enum CrudType
{
    One,
    Many,
    All
}

public enum CrudAction
{
    Create,
    Update,
    Delete,
}

public class CRUD<T>(
    TicTacToe_Context _dbContext,
    DbSet<T> _DbSet,
    List<string> _DbSetPKeys) where T : class
{
    readonly TicTacToe_Context dbContext = _dbContext;
    readonly DbSet<T> DbSet = _DbSet;
    readonly List<string> DbSetPKeys = _DbSetPKeys;

    readonly List<PropertyInfo> DbSetPropInfo = EntityHelper.GetClassPropInfo<T>();

    void UpdateDbSet(T item, Dictionary<string, object> props)
    {
        foreach (var pInf in DbSetPropInfo)
            pInf.SetValue(item, props[pInf.Name]);
    }

    public async Task<Result> Do(
        CrudType ct, 
        CrudAction action,
        Dictionary<string, object> oldProps,
        Dictionary<string, object> newProps)
    {
        switch (ct)
        {
            case CrudType.All:
            case CrudType.Many:
                throw new NotImplementedException($"CrudType '{ct}'");
            case CrudType.One:
                break;
            default:
                throw new Exception($"Unknown CrudType '{ct}'");
        }

        if(action == CrudAction.Create)
        {
            return new Result();
        }

        object[] PKeyValues = DbSetPKeys.Select(pk => oldProps[pk]).ToArray();

        T? item = await DbSet.FindAsync(PKeyValues);

        if (item == null)
        {
            string outp = "";
            foreach (var val in PKeyValues) 
                outp += val.ToString() + ' ';
            throw new Exception($"CRUD.Do : item not found on {outp}");
        }

        // CrudAction.Delete -> DeleteDbSet()
        // CrudAction.Update

        UpdateDbSet(item, newProps);

        using var transaction = await dbContext.Database
            .BeginTransactionAsync(System.Data.IsolationLevel.RepeatableRead);

        var infKey = "CRUD.Do : ";
        var result = new Result
        {
            info = new Dictionary<string, List<string>>{{infKey, []}}
        };

        try
        {
            int cnt = await dbContext.SaveChangesAsync();

            result.info[infKey].Add($"SaveChangesAsync success, {cnt} row affected");

            await transaction.CommitAsync();

            result.type = ResType.OK;
            result.info[infKey].Add("CommitAsync success");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();

            UpdateDbSet(item, oldProps);

            result.type = ResType.Error;
            result.info[infKey].Add("Exception: " + ex.Message);
        }

        return result;
    }

}
