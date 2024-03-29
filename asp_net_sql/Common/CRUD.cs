﻿using asp_net_sql.Data;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
//
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
    List<string> _DbSetPKeys) where T : class, new()
{
    readonly TicTacToe_Context dbContext = _dbContext;
    readonly DbSet<T> DbSet = _DbSet;
    readonly List<string> DbSetPKeys = _DbSetPKeys;

    readonly List<PropertyInfo> DbSetPropInfo = EntityHelper.GetClassPropInfo<T>();

    async Task AddDbSet(Dictionary<string, object>? props)
    {
        T item = new();

        if(props != null)
        foreach (var pInf in DbSetPropInfo)
            pInf.SetValue(item, props[pInf.Name]);

        await DbSet.AddAsync(item);
    }

    void DeleteDbSet(T item)
    {
        DbSet.Remove(item);
    }

    void UpdateDbSet(T item, Dictionary<string, object> props)
    {
        foreach (var pInf in DbSetPropInfo)
            pInf.SetValue(item, props[pInf.Name]);
    }

    async Task<T> GetEntityByPkey(Dictionary<string, object> props)
    {
        object[] PKeyValues = DbSetPKeys.Select(pk => props[pk]).ToArray();
        T? item = await DbSet.FindAsync(PKeyValues) ?? throw new Exception
                ($"CRUD.GetEntityByPkey : not found by '{Utl.DictToString(props, ", ")}'");

        return item;
    }

    public async Task<Result> Do(
        CrudType ct, 
        CrudAction ca,
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

        T? item = null;

        switch (ca)
        {
            case CrudAction.Create:
                await AddDbSet(newProps);
                break;
            case CrudAction.Update:
                item = await GetEntityByPkey(oldProps);
                UpdateDbSet(item, newProps);
                break;
            case CrudAction.Delete:
                item = await GetEntityByPkey(oldProps);
                DeleteDbSet(item);
                break;
        }

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

            if(ca == CrudAction.Update && item != null) 
                UpdateDbSet(item, oldProps);

            result.AddExInfo(ex, infKey);
        }

        return result;
    }

}
