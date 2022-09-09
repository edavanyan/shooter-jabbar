using System;
using System.Reflection;

public static class EnumExtension
{
    public static string GetStringValue(this Enum value) {
        Type type = value.GetType();
        FieldInfo fieldInfo = type.GetField(value.ToString());

        StringValueAttribute attrib = fieldInfo.GetCustomAttribute(
            typeof(StringValueAttribute), false) as StringValueAttribute;

        return attrib.StringValue;
    }
}

public class StringValueAttribute : Attribute
{
    public string StringValue
    {
        get;
    }

    public StringValueAttribute(string val)
    {
        StringValue = val;
    }
}
