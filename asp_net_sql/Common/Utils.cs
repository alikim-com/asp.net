using System.Reflection;
using Microsoft.EntityFrameworkCore;
//
using asp_net_sql.Data;
//
namespace asp_net_sql.Common;

public enum ResType
{
    None,
    OK,
    Error
}

public class Result(
    ResType _type = ResType.None,
    Dictionary<string, List<string>>? _info = null)
{

    public ResType type = _type;
    public Dictionary<string, List<string>> info = _info ?? [];
}

public static class EntityHelper
{
    public static Type MakeGenericType(Type genericType, string typeArgument)
    {
        Type? argument = Type.GetType(typeArgument) ?? throw new Exception
            ($"EntityHelper.MakeGenericType : argument type '{typeArgument}' not found");

        return genericType.MakeGenericType(argument);
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
