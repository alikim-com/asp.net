using System.Reflection.Emit;
using System.Reflection;

namespace test_enum;

public interface IEnum { }

//internal class ClsEnumType1 : IEnum
//{
//    static internal readonly int EnumValue = 0;
//    internal readonly int Value;
//    internal ClsEnumType1(int _value) { Value = _value; }
//}

//internal class ClsEnumType2 : IEnum
//{
//    static internal readonly int EnumValue = 0;
//    internal readonly int Value;
//    internal ClsEnumType2(int _value) { Value = _value; }
//}

internal class Program
{
    enum EnumType1 { Value = 0 }
    enum EnumType2 { Value = 0 }

    static void Main(string[] args)
    {
        var dict = new Dictionary<Enum, string>
        {
            { EnumType1.Value, "Type1" },
            { EnumType2.Value, "Type2" }
        };

        Console.WriteLine(dict[(EnumType1)0]);
        Console.WriteLine(dict[(EnumType2)0]);

        // output
        // Test1
        // Test2

        //var enumType1Value = new ClsEnumType1(ClsEnumType1.EnumValue);
        //var enumType2Value = new ClsEnumType2(ClsEnumType2.EnumValue);

        //var dict2 = new Dictionary<IEnum, string>
        //{
        //    { enumType1Value, "Type1" },
        //    { enumType2Value, "Type2" }
        //};

        //Console.WriteLine(dict2[enumType1Value]);
        //Console.WriteLine(dict2[enumType2Value]);


        Type ClsEnumType1 = DynamicType.GenerateEnumClass("DynamicEnums", "ClsEnumType1", "EnumValue", 0);

        object? instance = DynamicType.CreateInstance(ClsEnumType1, 42);

        Console.WriteLine(instance?.GetType().Name);  // Output: ClsEnumType1
    }

}

public class DynamicType
{
    static public Type GenerateEnumClass(string _assemblyName, string className, string fieldName, int fieldValue)
    {
        var assemblyName = new AssemblyName(_assemblyName);
        var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
        var moduleBuilder = assemblyBuilder.DefineDynamicModule("ManifestModule");

        // Create a type
        var typeBuilder = moduleBuilder.DefineType(
            className,
            TypeAttributes.Public | TypeAttributes.Class,
            null,
            new Type[] { typeof(IEnum) });

        // Create a static readonly field
        var fieldBuilder = typeBuilder.DefineField(
            fieldName,
            typeof(int),
            FieldAttributes.Static | FieldAttributes.InitOnly | FieldAttributes.Assembly);

        // Set the initial value of the field
        fieldBuilder.SetConstant(fieldValue);

        // Create a constructor
        var constructorBuilder = typeBuilder.DefineConstructor(
            MethodAttributes.Public | MethodAttributes.HideBySig,
            CallingConventions.Standard,
            new Type[] { typeof(int) });

        // Generate IL code for the constructor
        var ilGenerator = constructorBuilder.GetILGenerator();
        ilGenerator.Emit(OpCodes.Ldarg_0);
        ilGenerator.Emit(OpCodes.Ldarg_1);
        ilGenerator.Emit(OpCodes.Stfld, fieldBuilder);
        ilGenerator.Emit(OpCodes.Ret);

        // Create the type
        var generatedType = typeBuilder.CreateType();

        return generatedType;
    }

    static public object? CreateInstance(Type type, int value)
    {
        // Create an instance of the dynamically generated type
        var instance = Activator.CreateInstance(type, value);
        return instance;
    }
}