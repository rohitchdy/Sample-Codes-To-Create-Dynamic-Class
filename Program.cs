using System.Reflection;
using System.Reflection.Emit;
namespace TypeBuilderNamespace;

public static class MyTypeBuilder
{
    public static List<Field> fields = new List<Field>();

    public static void Main()
    {
        var field = new Field()
        {
            FieldId = 1,
            FieldName = "EmployeeId",
            FieldType = "int"
        };
        fields.Add(field);
        var field1 = new Field()
        {
            FieldId = 2,
            FieldName = "EmployeeName",
            FieldType = "string"
        };
        fields.Add(field1);

        var field2 = new Field()
        {
            FieldId = 3,
            FieldName = "EmployeeAddress",
            FieldType = "string"
        };
        fields.Add(field2);

        var field3 = new Field()
        {
            FieldId = 4,
            FieldName = "EmployeeSalary",
            FieldType = "decimal"
        };
        fields.Add(field3);

        //These Two Are The Required Output
        var myType = CompileResultType(fields);
        var myObject = Activator.CreateInstance(myType);
    }
    public static Type CompileResultType(List<Field> fields)
    {
        TypeBuilder tb = GetTypeBuilder();
        ConstructorBuilder constructor = tb.DefineDefaultConstructor(MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName);

        // NOTE: assuming your list contains Field objects with fields FieldName(string) and FieldType(Type)
        foreach (var field in fields)
            CreateProperty(tb, field.FieldName, GetType(field.FieldType));

        Type objectType = tb.CreateType();
        return objectType;
    }

    private static TypeBuilder GetTypeBuilder()
    {
        var typeSignature = "MyDynamicType";
        var an = new AssemblyName(typeSignature);
        AssemblyBuilder assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(an, AssemblyBuilderAccess.Run);
        ModuleBuilder moduleBuilder = assemblyBuilder.DefineDynamicModule("MainModule");
        TypeBuilder tb = moduleBuilder.DefineType(typeSignature,
                TypeAttributes.Public |
                TypeAttributes.Class |
                TypeAttributes.AutoClass |
                TypeAttributes.AnsiClass |
                TypeAttributes.BeforeFieldInit |
                TypeAttributes.AutoLayout,
                null);
        return tb;
    }

    private static void CreateProperty(TypeBuilder tb, string propertyName, Type propertyType)
    {
        FieldBuilder fieldBuilder = tb.DefineField("_" + propertyName, propertyType, FieldAttributes.Private);

        PropertyBuilder propertyBuilder = tb.DefineProperty(propertyName, PropertyAttributes.HasDefault, propertyType, null);
        MethodBuilder getPropMthdBldr = tb.DefineMethod("get_" + propertyName, MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig, propertyType, Type.EmptyTypes);
        ILGenerator getIl = getPropMthdBldr.GetILGenerator();

        getIl.Emit(OpCodes.Ldarg_0);
        getIl.Emit(OpCodes.Ldfld, fieldBuilder);
        getIl.Emit(OpCodes.Ret);

        MethodBuilder setPropMthdBldr =
            tb.DefineMethod("set_" + propertyName,
              MethodAttributes.Public |
              MethodAttributes.SpecialName |
              MethodAttributes.HideBySig,
              null, new[] { propertyType });

        ILGenerator setIl = setPropMthdBldr.GetILGenerator();
        Label modifyProperty = setIl.DefineLabel();
        Label exitSet = setIl.DefineLabel();

        setIl.MarkLabel(modifyProperty);
        setIl.Emit(OpCodes.Ldarg_0);
        setIl.Emit(OpCodes.Ldarg_1);
        setIl.Emit(OpCodes.Stfld, fieldBuilder);

        setIl.Emit(OpCodes.Nop);
        setIl.MarkLabel(exitSet);
        setIl.Emit(OpCodes.Ret);

        propertyBuilder.SetGetMethod(getPropMthdBldr);
        propertyBuilder.SetSetMethod(setPropMthdBldr);
    }
    public static Type GetType(string FieldType)
    {
        switch (FieldType)
        {
            case "int": return typeof(int);
            case "string": return typeof(string);
            case "decimal": return typeof(decimal);
            case "double": return typeof(double);
            case "float": return typeof(float);
            case "boolean": return typeof(bool);
        }
        return null;
    }
}


public class Field
{
    public int FieldId { get; set; }
    public string FieldName { get; set; }
    public string FieldType { get; set; }
}
