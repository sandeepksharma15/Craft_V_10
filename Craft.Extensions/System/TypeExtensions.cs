using System.Reflection;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace System;
#pragma warning restore IDE0130 // Namespace does not match folder structure

public static class TypeExtensions
{
    extension (Type? type)
    {
        /// <summary>
        /// Retrieves a list of class names within the current AppDomain that are inherited from the specified <paramref name="type"/>
        /// and marked with the specified attribute <typeparamref name="T"/>.
        /// </summary>
        public IList<string> GetClassesWithAttribute<T>() where T : Attribute
        {
            if (type is null) return Array.Empty<string>();
            return AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(t => type.IsAssignableFrom(t) && t.HasAttribute<T>() && t.IsClass && !t.IsAbstract)
                .Select(t => t.Name)
                .ToList();
        }

        /// <summary>
        /// Gets a list of class names that are inherited from the specified <paramref name="type"/>
        /// and do not have the specified attribute <typeparamref name="T"/>.
        /// </summary>
        public IList<string> GetClassesWithoutAttribute<T>() where T : Attribute
        {
            if (type is null) return Array.Empty<string>();
            return AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(t => type.IsAssignableFrom(t) && !t.HasAttribute<T>() && t.IsClass && !t.IsAbstract)
                .Select(t => t.Name)
                .ToList();
        }

        /// <summary>
        /// Gets a list of class names that are inherited from the specified <paramref name="type"/>.
        /// </summary>
        public IList<string> GetInheritedClasses()
        {
            if (type is null) return Array.Empty<string>();
            return AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(t => type.IsAssignableFrom(t) && t.IsClass && !t.IsAbstract && type != t)
                .Select(t => t.Name)
                .ToList();
        }

        /// <summary>
        /// Checks if the specified <paramref name="type"/> has the specified attribute <typeparamref name="T"/>.
        /// </summary>
        public bool HasAttribute<T>() where T : Attribute =>
            type?.GetCustomAttributes(typeof(T), true).Length > 0;

        /// <summary>
        /// Determines whether the specified <paramref name="type"/> is a nullable type.
        /// </summary>
        public bool IsNullable() =>
            type is not null && Nullable.GetUnderlyingType(type) != null;

        /// <summary>
        /// Gets the non-nullable type from the provided type. If the type is nullable,
        /// returns the underlying non-nullable type; otherwise, returns the original type.
        /// </summary>
        public Type GetNonNullableType() =>
            type is null ? typeof(object) : Nullable.GetUnderlyingType(type) ?? type;

        /// <summary>
        /// Checks if the specified <paramref name="type"/> represents a numeric type.
        /// </summary>
        /// <param name="type">The type to check.</param>
        /// <returns>True if the type is numeric; otherwise, false.</returns>
        public bool IsNumeric()
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
                        return Nullable.GetUnderlyingType(type)!.IsNumeric();
                    return false;

                default:
                    return false;
            }
        }

        /// <summary>
        /// Determines whether the current type is compatible with the specified type.
        /// </summary>
        public bool IsCompatibleWith(Type? type2) =>
            type is not null && type2 is not null &&
            (type.IsAssignableFrom(type2) || type2.IsAssignableFrom(type));

        /// <summary>
        /// Determines whether the current type is not compatible with the specified type.
        /// </summary>
        public bool IsNotCompatibleWith(Type? type2)
            => !(type!.IsCompatibleWith(type2));

        /// <summary>
        /// Determines whether the current type implements the specified interface.
        /// </summary>
        public bool HasImplementedInterface(Type? baseType) =>
            type is not null && baseType?.IsInterface == true && type.GetInterfaces().Contains(baseType);

        /// <summary>
        /// Determines whether the current type is derived from the specified base class or implements the specified interface.
        /// </summary>
        public bool IsDerivedFromClass(Type baseClassType)
        {
            if (type == null || baseClassType == null) return false;

            if (type == baseClassType) return true;

            if (baseClassType.IsInterface)
                return type.GetInterfaces().Contains(baseClassType);

            return type.BaseType != null && type.BaseType.IsDerivedFromClass(baseClassType);
        }

        /// <summary>
        /// Retrieves the name of the class represented by the current instance.
        /// </summary>
        public string? GetClassName() =>
            type?.ToString().GetStringAfterLastDelimiter();
    }


    extension(MemberInfo? member)
    {
        /// <summary>
        /// Retrieves the underlying type of the member represented by the current <see cref="MemberInfo"/>.
        /// </summary>
        public Type? GetMemberUnderlyingType()
        {
            return member?.MemberType switch
            {
                MemberTypes.Field => ((FieldInfo)member).FieldType,
                MemberTypes.Property => ((PropertyInfo)member).PropertyType,
                MemberTypes.Event => ((EventInfo)member).EventHandlerType,
                _ => throw new ArgumentException("MemberInfo must be if type FieldInfo, PropertyInfo or EventInfo", nameof(member)),
            };
        }
    }
}
