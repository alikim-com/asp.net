﻿using System.Reflection;
using Microsoft.EntityFrameworkCore;
//
using asp_net_sql.Data;
using System.Text.Json;
//
namespace asp_net_sql.Common;

public static class StringExtensions
{
    public static string Capitalise(this string input) => string.IsNullOrEmpty(input) ?
        input : char.ToUpper(input[0]) + input[1..];
}

public class Utl
{
    public static string DictToString<TKey, TValue>(
        Dictionary<TKey, TValue> dict,
        string? sep = null) where TKey : notnull =>
        string.Join(
            sep ?? Environment.NewLine,
            dict.Select(kvp => $"Key: {kvp.Key}, Value: {kvp.Value}")
        );
}

public enum ResType
{
    None,
    OK,
    Error
}

public class Result
{
    public ResType type;
    public Dictionary<string, List<string>> info;

    public Result(
        ResType _type = ResType.None,
        Dictionary<string, List<string>>? _info = null)
    {
        type = _type;
        info = _info ?? [];
    }

    public Result(
        ResType _type,
        string singleKey,
        string singleVal)
    {
        type = _type;
        info = new Dictionary<string, List<string>>()
        { { singleKey, [singleVal] } };
    }

    public void AddExeptionInfo(string key, Exception ex)
    {
        type = ResType.Error;
        info[key].Add("Exception: " + ex.Message);
        if (ex.InnerException != null)
            info[key].Add("Inner exception: " + ex.InnerException.Message);
    }
}

public class APIPacket : Result
{
    public Dictionary<string, string>? keyValuePairs;
    public string? status;
    public string? message;
    public Guid? guid;

    public APIPacket(
    Dictionary<string, string>? _keyValuePairs = null,
    string? _status = null,
    string? _message = null,
    Guid? _guid = null)
    {
        keyValuePairs = _keyValuePairs;
        status = _status;
        message = _message;
        guid = _guid ?? Guid.NewGuid();
    }

    /// <summary>
    /// For JsonSerializer.Deserialize
    /// </summary>
    public APIPacket() { }
}

class Post
{
    public static JsonSerializerOptions includeFields = new()
    {
        IncludeFields = true
    };

    public static void Context(
            HttpRequest req,
            string url,
            APIPacket data,
            out string apiEndpoint,
            out HttpClient client,
            out HttpContent content,
            out string json)
    {
        string selfURI = req.Scheme + "://" + req.Host.ToUriComponent();

        apiEndpoint = selfURI + url;

        client = new();

        json = JsonSerializer.Serialize(data, includeFields);

        content = new StringContent(
            json,
            System.Text.Encoding.UTF8,
            "application/json");
    }
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
