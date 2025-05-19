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
        /// <typeparam name="T">The type of attribute to check for.</typeparam>
        /// <param name="type">The base type to check for inheritance.</param>
        /// <returns>A list of class names with the specified attribute.</returns>
        public IList<string> GetClassesWithAttribute<T>() where T : Attribute
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
        public IList<string> GetClassesWithoutAttribute<T>() where T : Attribute
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
        public IList<string> GetInheritedClasses()
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
        /// Checks if the specified <paramref name="type"/> has the specified attribute <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type of attribute to check for.</typeparam>
        /// <param name="type">The type to check for the presence of the attribute.</param>
        /// <returns>True if the attribute is present; otherwise, false.</returns>
        public bool HasAttribute<T>() where T : Attribute
        {
            if (type is null) return false;

            return type.GetCustomAttributes(typeof(T), true).Length > 0;
        }

        /// <summary>
        /// Determines whether the specified <paramref name="type"/> is a nullable type.
        /// </summary>
        /// <param name="type">The type to check for nullable.</param>
        /// <returns>True if the type is nullable; otherwise, false.</returns>
        public bool IsNullable()
            => Nullable.GetUnderlyingType(type!) != null;

        /// <summary>
        /// Gets the non-nullable type from the provided type. If the type is nullable,
        /// returns the underlying non-nullable type; otherwise, returns the original type.
        /// </summary>
        /// <param name="type">The input type.</param>
        /// <returns>The non-nullable type.</returns>
        public Type GetNonNullableType()
        {
            return type is null 
                ? typeof(object) 
                : Nullable.GetUnderlyingType(type) ?? type;
        }

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
        /// <param name="type2">The type to compare with the current type. Cannot be <see langword="null"/>.</param>
        /// <returns><see langword="true"/> if the current type is assignable from <paramref name="type2"/>  or <paramref
        /// name="type2"/> is assignable from the current type; otherwise, <see langword="false"/>.</returns>
        public bool IsCompatibleWith(Type? type2)
            => type!.IsAssignableFrom(type2) || type2!.IsAssignableFrom(type);
    
        /// <summary>
        /// Determines whether the current type is not compatible with the specified type.
        /// </summary>
        /// <param name="type2">The type to compare for compatibility. Can be <see langword="null"/>.</param>
        /// <returns><see langword="true"/> if the current type is not compatible with <paramref name="type2"/>;  otherwise, <see
        /// langword="false"/>.</returns>
        public bool IsNotCompatibleWith(Type? type2)
            => !(type!.IsCompatibleWith(type2));

        /// <summary>
        /// Determines whether the current type implements the specified interface.
        /// </summary>
        /// <remarks>This method checks whether the current type directly or indirectly implements the
        /// specified interface. If <paramref name="baseType"/> is not an interface, the method will return <see
        /// langword="false"/>.</remarks>
        /// <param name="baseType">The interface type to check for. Must be a non-null interface type.</param>
        /// <returns><see langword="true"/> if the current type implements the specified interface; otherwise, <see
        /// langword="false"/>.</returns>
        public bool HasImplementedInterface(Type? baseType)
        {
            if (type == null || baseType?.IsInterface != true)
                return false;

            return type.GetInterfaces().Contains(baseType);
        }

        public bool IsDerivedFromClass(Type baseClassType)
        {
            // Check if the types are the same
            if (type == baseClassType)
                return true;

            if (baseClassType.IsInterface)
                return type!.GetInterfaces().Contains(baseClassType);

            // Check if the base type of the derived class is not null
            if (type!.BaseType != null)
                return type.BaseType.IsDerivedFromClass(baseClassType);

            return false;
        }

        /// <summary>
        /// Retrieves the name of the class represented by the current instance.
        /// </summary>
        /// <remarks>The returned class name is extracted from the <c>type</c> field and processed to
        /// remove any preceding namespace or delimiter information.</remarks>
        /// <returns>A string containing the class name if the <c>type</c> field is not <see langword="null"/>; otherwise, <see
        /// langword="null"/>.</returns>
        public string? GetClassName()
        {
            if (type is null) return null;

            return type.ToString().GetStringAfterLastDelimiter();
        }
    }


    extension(MemberInfo? member)
    {
        /// <summary>
        /// Retrieves the underlying type of the member represented by the current <see cref="MemberInfo"/>.
        /// </summary>
        /// <remarks>This method determines the type of the member based on its <see
        /// cref="MemberInfo.MemberType"/>. Supported member types include fields, properties, and events. If the member
        /// is a field, the field's type is returned. If the member is a property, the property's type is returned. If
        /// the member is an event, the event handler type is returned.</remarks>
        /// <returns>The <see cref="Type"/> of the underlying member, or <see langword="null"/> if the member is <see
        /// langword="null"/>.</returns>
        /// <exception cref="ArgumentException">Thrown if the member is not of type <see cref="FieldInfo"/>, <see cref="PropertyInfo"/>, or <see
        /// cref="EventInfo"/>.</exception>
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
