using System.Reflection;


namespace Craft.Extensions.Tests.System;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class TestAttribute : Attribute;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class AnotherAttribute : Attribute;

public class BaseClass;

[Test]
public class DerivedClass : BaseClass, IInterface, IGenericInterface<string>;

[Test]
public class ClassA : BaseClass;

[Another]
public class ClassB : BaseClass;

public interface IInterface;

public interface IGenericInterface<T>;

public interface INonGenericInterface;

#pragma warning disable IDE0051, RCS1213, CS0169, CS0067

public class MyTestClass
{
    private readonly int _myField;

    public string? MyProperty { get; set; }

    public event EventHandler? MyEvent;
}

public class MyClass
{
    public int AnotherProperty { get; set; }
    public string? PropertyName { get; set; }
}

#pragma warning restore CS0169, RCS1213, IDE0051, CS0067

public class TypeTests
{
    [Theory]
    [InlineData(typeof(BaseClass), false)]
    [InlineData(typeof(DerivedClass), true)]
    [InlineData(typeof(IInterface), false)]
    [InlineData(typeof(IGenericInterface<string>), false)]
    [InlineData(typeof(INonGenericInterface), false)]
    public void HasAttribute_ShouldReturnCorrectResult(Type type, bool expectedResult)
    {
        // Act
        var result = type.HasAttribute<TestAttribute>();

        // Assert
        Assert.Equal(expectedResult, result);
    }

    [Theory]
    [InlineData(typeof(int), false)]                // Non-nullable type
    [InlineData(typeof(string), false)]             // Non-nullable reference type
    [InlineData(typeof(int?), true)]                // Nullable value type
    [InlineData(typeof(double?), true)]             // Nullable value type (different type)
    [InlineData(typeof(object), false)]              // Reference type
    [InlineData(typeof(BaseClass), false)]          // Non-nullable user-defined reference type
    [InlineData(typeof(DerivedClass), false)]       // Non-nullable derived user-defined reference type
    [InlineData(typeof(IInterface), false)]         // Non-nullable interface
    [InlineData(typeof(INonGenericInterface), false)]   // Non-nullable non-generic interface
    public void IsNullable_ShouldReturnExpectedResult(Type type, bool expectedResult)
    {
        // Act
        var result = type.IsNullable();

        // Assert
        Assert.Equal(expectedResult, result);
    }

    [Theory]
    [InlineData(typeof(int), typeof(int))] // Non-nullable type
    [InlineData(typeof(int?), typeof(int))] // Nullable type
    [InlineData(typeof(string), typeof(string))] // Reference type
    [InlineData(typeof(double?), typeof(double))] // Nullable value type
    [InlineData(typeof(decimal?), typeof(decimal))] // Nullable value type
    [InlineData(typeof(object), typeof(object))] // Generic type
    [InlineData(typeof(MyClass), typeof(MyClass))] // Custom class type
    public void GetNonNullableType_ShouldReturnNonNullableType(Type inputType, Type expectedType)
    {
        // Arrange

        // Act
        Type result = inputType.GetNonNullableType();

        // Assert
        Assert.Equal(expectedType, result);
    }

    [Theory]
    [InlineData(typeof(BaseClass), typeof(DerivedClass), true)]
    [InlineData(typeof(BaseClass), typeof(BaseClass), true)]
    [InlineData(typeof(IInterface), typeof(DerivedClass), true)]
    [InlineData(typeof(IGenericInterface<string>), typeof(DerivedClass), true)]
    [InlineData(typeof(INonGenericInterface), typeof(DerivedClass), false)]
    [InlineData(typeof(DerivedClass), typeof(BaseClass), false)]
    [InlineData(typeof(DerivedClass), typeof(IInterface), false)]
    [InlineData(typeof(DerivedClass), typeof(IGenericInterface<string>), false)]
    [InlineData(typeof(DerivedClass), typeof(INonGenericInterface), false)]
    public void IsDerivedFromClass_ShouldReturnExpectedResult(Type baseType, Type derivedType, bool expectedResult)
    {
        // Act
        bool result = derivedType.IsDerivedFromClass(baseType);

        // Assert
        Assert.Equal(expectedResult, result);
    }

    [Theory]
    [InlineData(typeof(DerivedClass), typeof(IInterface), true)]
    [InlineData(typeof(DerivedClass), typeof(INonGenericInterface), false)]
    [InlineData(typeof(BaseClass), typeof(IInterface), false)]
    [InlineData(null, typeof(IInterface), false)]
    [InlineData(typeof(DerivedClass), null, false)]
    [InlineData(typeof(DerivedClass), typeof(BaseClass), false)]
    public void HasImplementedInterface_ShouldReturnExpectedResult(Type? derivedType, Type? baseType, bool expectedResult)
    {
        // Act
        bool? result = derivedType?.HasImplementedInterface(baseType);

        // Assert
        Assert.Equal(expectedResult, result);
    }

    [Theory]
    [InlineData(typeof(byte), true)]
    [InlineData(typeof(sbyte), true)]
    [InlineData(typeof(ushort), true)]
    [InlineData(typeof(uint), true)]
    [InlineData(typeof(ulong), true)]
    [InlineData(typeof(short), true)]
    [InlineData(typeof(int), true)]
    [InlineData(typeof(long), true)]
    [InlineData(typeof(decimal), true)]
    [InlineData(typeof(double), true)]
    [InlineData(typeof(float), true)]
    [InlineData(typeof(object), false)]
    [InlineData(typeof(string), false)]
    [InlineData(typeof(bool), false)]
    [InlineData(typeof(char), false)]
    [InlineData(typeof(DateTime), false)]
    [InlineData(typeof(TimeSpan), false)]
    public void IsNumeric_ShouldReturnCorrectValue(Type type, bool expected)
    {
        // Act
        var actual = type.IsNumeric();

        // Assert
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void GetClassesWithAttribute_NullType_ShouldReturnNull()
    {
        // Arrange
        Type? type = null;

        // Act
        var result = type?.GetClassesWithAttribute<TestAttribute>();

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public void GetClassesWithAttribute_NoMatchingClasses_ShouldReturnEmptyList()
    {
        // Arrange
        Type type = typeof(ClassB);

        // Act
        var result = type.GetClassesWithAttribute<TestAttribute>();

        /// Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public void GetClassesWithAttribute_MatchingClasses_ShouldReturnClassNames()
    {
        // Arrange
        Type type = typeof(BaseClass);

        // Act
        var result = type.GetClassesWithAttribute<TestAttribute>();

        // Assert
        var expected = new[] { "DerivedClass", "ClassA" };
        Assert.Equal(expected, result);
    }

    [Fact]
    public void GetClassesWithoutAttribute_NullType_ShouldReturnNull()
    {
        // Arrange
        Type? type = null;

        // Act
        var result = type?.GetClassesWithoutAttribute<TestAttribute>();

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public void GetClassesWithoutAttribute_NoMatchingClasses_ShouldReturnEmptyList()
    {
        // Arrange
        Type type = typeof(ClassB);

        // Act
        var result = type.GetClassesWithoutAttribute<AnotherAttribute>();

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public void GetClassesWithoutAttribute_MatchingClasses_ShouldReturnClassNames()
    {
        // Arrange
        Type type = typeof(BaseClass);

        // Act
        var result = type.GetClassesWithoutAttribute<AnotherAttribute>();

        // Assert
        var expected = new[] { "DerivedClass", "ClassA" };
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(typeof(BaseClass), 3)]
    [InlineData(typeof(DerivedClass), 0)]
    public void GetInheritedClasses_ShouldReturnCorrectClasses(Type baseClassType, int expectedCount)
    {
        // Act
        var result = baseClassType.GetInheritedClasses();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedCount, result.Count);
    }

    [Fact]
    public void GivenFieldInfo_ShouldReturnFieldType()
    {
        // Arrange
        var fieldInfo = typeof(MyTestClass).GetField("_myField", BindingFlags.NonPublic | BindingFlags.Instance);

        // Act
        var underlyingType = fieldInfo?.GetMemberUnderlyingType();

        // Assert
        Assert.Equal(typeof(int), underlyingType);
    }

    [Fact]
    public void GivenPropertyInfo_ShouldReturnPropertyType()
    {
        // Arrange
        var propertyInfo = typeof(MyTestClass).GetProperty("MyProperty");

        // Act
        var underlyingType = propertyInfo?.GetMemberUnderlyingType();

        // Assert
        Assert.Equal(typeof(string), underlyingType);
    }

    [Fact]
    public void GivenEventInfo_ShouldReturnEventHandlerType()
    {
        // Arrange
        var eventInfo = typeof(MyTestClass).GetEvent("MyEvent");

        // Act
        var underlyingType = eventInfo?.GetMemberUnderlyingType();

        // Assert
        Assert.Equal(typeof(EventHandler), underlyingType);
    }

    [Fact]
    public void GivenInvalidMemberType_ShouldThrowArgumentException()
    {
        // Arrange
        var invalidMemberInfo = typeof(MyTestClass);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => invalidMemberInfo.GetMemberUnderlyingType());
    }
}
