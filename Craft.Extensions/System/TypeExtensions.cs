using System.Reflection;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace System;
#pragma warning restore IDE0130 // Namespace does not match folder structure

public static class TypeExtensions
{
    /// <summary>
    /// Retrieves a list of class names within the current AppDomain that are inherited from the specified <paramref name="type"/>
    /// and marked with the specified attribute <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The type of attribute to check for.</typeparam>
    /// <param name="type">The base type to check for inheritance.</param>
    /// <returns>A list of class names with the specified attribute.</returns>
    public static IList<string> GetClassesWithAttribute<T>(this Type type) where T : Attribute
    {
        if (type is null) return [];

        // Get the list of all the types inherited from class type having attribute T
        var classes = AppDomain.CurrentDomain.GetAssemblies()
               .SelectMany(s => s.GetTypes())
               .Where(t => type.IsAssignableFrom(t) && t.HasAttribute<T>() && t.IsClass && !t.IsAbstract)
               .Select(t => t.Name)
               .ToList();

        return classes;
    }

    /// <summary>
    /// Gets a list of class names that are inherited from the specified <paramref name="type"/>
    /// and do not have the specified attribute <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The type of attribute to check for.</typeparam>
    /// <param name="type">The base type to check for inherited classes without the attribute.</param>
    /// <returns>A list of class names that meet the criteria.</returns>
    public static IList<string> GetClassesWithoutAttribute<T>(this Type type) where T : Attribute
    {
        if (type is null) return [];

        // Get the list of all the types inherited from class type not having attribute T
        var classes = AppDomain.CurrentDomain.GetAssemblies()
               .SelectMany(s => s.GetTypes())
               .Where(t => type.IsAssignableFrom(t) && !t.HasAttribute<T>() && t.IsClass && !t.IsAbstract)
               .Select(t => t.Name)
               .ToList();

        return classes;
    }

    /// <summary>
    /// Gets a list of class names that are inherited from the specified <paramref name="type"/>.
    /// </summary>
    /// <param name="type">The base type to check for inherited classes.</param>
    /// <returns>A list of class names that are inherited from the base type.</returns>
    public static IList<string> GetInheritedClasses(this Type type)
    {
        if (type is null) return [];

        // Get the list of all the types inherited from class type
        var classes = AppDomain.CurrentDomain.GetAssemblies()
               .SelectMany(s => s.GetTypes())
               .Where(t => type.IsAssignableFrom(t) && t.IsClass && !t.IsAbstract && (type != t))
               .Select(t => t.Name)
               .ToList();

        return classes;
    }

    /// <summary>
    /// Retrieves the underlying type of the specified member.
    /// </summary>
    /// <param name="member">The <see cref="MemberInfo"/> instance representing the member whose underlying type is to be determined. Must be
    /// of type <see cref="FieldInfo"/>, <see cref="PropertyInfo"/>, or <see cref="EventInfo"/>.</param>
    /// <returns>The <see cref="Type"/> of the member if it is a field, property, or event; otherwise, <see langword="null"/> if
    /// <paramref name="member"/> is <see langword="null"/>.</returns>
    /// <exception cref="ArgumentException">Thrown if <paramref name="member"/> is not of type <see cref="FieldInfo"/>, <see cref="PropertyInfo"/>, or <see
    /// cref="EventInfo"/>.</exception>
    public static Type? GetMemberUnderlyingType(this MemberInfo? member)
    {
        return member?.MemberType switch
        {
            MemberTypes.Field => ((FieldInfo)member!).FieldType,
            MemberTypes.Property => ((PropertyInfo)member!).PropertyType,
            MemberTypes.Event => ((EventInfo)member!).EventHandlerType,
            _ => throw new ArgumentException("MemberInfo must be if type FieldInfo, PropertyInfo or EventInfo", nameof(member)),
        };
    }

    /// <summary>
    /// Gets the non-nullable type from the provided type. If the type is nullable,
    /// returns the underlying non-nullable type; otherwise, returns the original type.
    /// </summary>
    /// <param name="type">The input type.</param>
    /// <returns>The non-nullable type.</returns>
    public static Type GetNonNullableType(this Type type)
        => Nullable.GetUnderlyingType(type) ?? type;

    /// <summary>
    /// Checks if the specified <paramref name="type"/> has the specified attribute <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The type of attribute to check for.</typeparam>
    /// <param name="type">The type to check for the presence of the attribute.</param>
    /// <returns>True if the attribute is present; otherwise, false.</returns>
    public static bool HasAttribute<T>(this Type type) where T : Attribute
    {
        if (type is null) return false;

        return type.GetCustomAttributes(typeof(T), true).Length > 0;
    }

    /// <summary>
    /// Checks if the specified <paramref name="derivedType"/> has implemented the specified interface <paramref name="baseType"/>.
    /// </summary>
    /// <param name="derivedType">The derived type to check.</param>
    /// <param name="baseType">The interface type to check for implementation.</param>
    /// <returns>True if the derived type implements the interface; otherwise, false.</returns>
    public static bool HasImplementedInterface(this Type? derivedType, Type? baseType)
    {
        if (derivedType == null || baseType?.IsInterface != true)
            return false;

        return derivedType.GetInterfaces().Contains(baseType);
    }

    /// <summary>
    /// Checks if the specified <paramref name="derivedClassType"/> is derived from the <paramref name="baseClassType"/>.
    /// </summary>
    /// <param name="derivedClassType">The derived class type to check.</param>
    /// <param name="baseClassType">The base class type.</param>
    /// <returns>True if the derived class is derived from the base class; otherwise, false.</returns>
    public static bool IsDerivedFromClass(this Type derivedClassType, Type baseClassType)
    {
        // Check if the types are the same
        if (derivedClassType == baseClassType)
            return true;

        if (baseClassType.IsInterface)
            return derivedClassType.GetInterfaces().Contains(baseClassType);

        // Check if the base type of the derived class is not null
        if (derivedClassType?.BaseType != null)
            return derivedClassType.BaseType.IsDerivedFromClass(baseClassType);

        return false;
    }

    /// <summary>
    /// Determines whether the specified <paramref name="type"/> is a nullable type.
    /// </summary>
    /// <param name="type">The type to check for nullable.</param>
    /// <returns>True if the type is nullable; otherwise, false.</returns>
    public static bool IsNullable(this Type type)
        => Nullable.GetUnderlyingType(type) != null;

    /// <summary>
    /// Checks if the specified <paramref name="type"/> represents a numeric type.
    /// </summary>
    /// <param name="type">The type to check.</param>
    /// <returns>True if the type is numeric; otherwise, false.</returns>
    public static bool IsNumeric(this Type? type)
    {
        if (type is null) return false;

        switch (Type.GetTypeCode(type))
        {
            case TypeCode.Byte:
            case TypeCode.SByte:
            case TypeCode.UInt16:
            case TypeCode.UInt32:
            case TypeCode.UInt64:
            case TypeCode.Int16:
            case TypeCode.Int32:
            case TypeCode.Int64:
            case TypeCode.Decimal:
            case TypeCode.Double:
            case TypeCode.Single:
                return true;

            case TypeCode.Object:
                if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
                    return Nullable.GetUnderlyingType(type).IsNumeric();
                return false;

            default:
                return false;
        }
    }

    /// <summary>
    /// Checks if the specified <paramref name="type"/> represents an Integral numeric type.
    /// </summary>
    /// <param name="type">The type to check.</param>
    /// <returns>True if the type is an Integral numeric type; otherwise, false.</returns>
    public static bool IsIntegral(this Type? type)
    {
        if (type is null) return false;

        switch (Type.GetTypeCode(type))
        {
            case TypeCode.Byte:
            case TypeCode.SByte:
            case TypeCode.UInt16:
            case TypeCode.UInt32:
            case TypeCode.UInt64:
            case TypeCode.Int16:
            case TypeCode.Int32:
            case TypeCode.Int64:
                return true;

            case TypeCode.Object:
                if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
                    return Nullable.GetUnderlyingType(type).IsIntegral();
                return false;

            default:
                return false;
        }
    }

    /// <summary>
    /// Checks if the specified <paramref name="type"/> represents a floating point type.
    /// </summary>
    /// <param name="type">The type to check.</param>
    /// <returns>True if the type is floating point; otherwise, false.</returns>
    public static bool IsFloating(this Type? type)
    {
        if (type is null) return false;

        switch (Type.GetTypeCode(type))
        {
            case TypeCode.Decimal:
            case TypeCode.Double:
            case TypeCode.Single:
                return true;

            case TypeCode.Object:
                if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
                    return Nullable.GetUnderlyingType(type).IsFloating();
                return false;

            default:
                return false;
        }
    }

    /// <summary>
    /// Determines whether the specified types are compatible for assignment.
    /// </summary>
    /// <remarks>This method checks if one type can be assigned to the other, considering inheritance and
    /// interface implementation.</remarks>
    /// <param name="type1">The first <see cref="Type"/> to compare.</param>
    /// <param name="type2">The second <see cref="Type"/> to compare.</param>
    /// <returns><see langword="true"/> if either <paramref name="type1"/> is assignable from <paramref name="type2"/>  or
    /// <paramref name="type2"/> is assignable from <paramref name="type1"/>; otherwise, <see langword="false"/>.</returns>
    public static bool IsCompatibleWith(this Type type1, Type type2)
        => type1.IsAssignableFrom(type2) || type2.IsAssignableFrom(type1);

    /// <summary>
    /// Determines whether the specified types are not compatible with each other.
    /// </summary>
    /// <remarks>Compatibility is determined based on the logic defined in the <see cref="IsCompatibleWith"/>
    /// method. This method is a negation of <see cref="IsCompatibleWith"/>.</remarks>
    /// <param name="type1">The first <see cref="Type"/> to compare.</param>
    /// <param name="type2">The second <see cref="Type"/> to compare.</param>
    /// <returns><see langword="true"/> if the types are not compatible; otherwise, <see langword="false"/>.</returns>
    public static bool IsNotCompatibleWith(this Type type1, Type type2)
        => !type1.IsCompatibleWith(type2);

    /// <summary>
    /// Retrieves the name of the class represented by the specified <see cref="Type"/> object.
    /// </summary>
    /// <param name="type">The <see cref="Type"/> instance for which to retrieve the class name. Cannot be <see langword="null"/>.</param>
    /// <returns>A <see cref="string"/> containing the class name, or <see langword="null"/> if the class name cannot be
    /// determined.</returns>
    public static string? GetClassName(this Type type)
        => type.ToString().GetStringAfterLastDelimiter();
}
