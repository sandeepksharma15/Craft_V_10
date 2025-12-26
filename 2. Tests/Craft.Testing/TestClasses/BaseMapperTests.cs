using AutoFixture;
using Craft.Testing.Helpers;
using Mapster;

namespace Craft.Testing.TestClasses;

public abstract class BaseMapperTests<T, EntityDTO, EntityVM, IType>
    where T : class, IType
    where EntityDTO : class, IType
    where EntityVM : class, IType
{
    protected virtual TClass? CreateInstance<TClass>() where TClass : class
    {
        var fixture = new Fixture();

        return fixture.Create<TClass>();
    }

    [Fact]
    public void DTO_To_Entity_IsValid()
    {
        // Arrange
        EntityDTO? entityDTO = CreateInstance<EntityDTO>();

        // Act
        T? entity = entityDTO?.Adapt<T>();

        // Assert
        entityDTO?.ShouldBeSameAs<IType>(entity!);
    }

    [Fact]
    public void Entity_To_VM_IsValid()
    {
        // Arrange
        T? entity = CreateInstance<T>();

        // Act
        EntityVM? entityVM = entity?.Adapt<EntityVM>();

        // Assert
        entity?.ShouldBeSameAs<IType>(entityVM!);
    }

    [Fact]
    public void VM_To_DTO_IsValid()
    {
        // Arrange
        EntityVM? entityVM = CreateInstance<EntityVM>();

        // Act
        EntityDTO? entityDTO = entityVM?.Adapt<EntityDTO>();

        // Assert
        entityVM?.ShouldBeSameAs<IType>(entityDTO!);
    }
}

public abstract class BaseMapperTests<T, EntityDTO, EntityVM, EntityModel, IType>
    where T : class, IType
    where EntityDTO : class, IType
    where EntityVM : class, IType
    where EntityModel : class, IType
{
    protected virtual TClass? CreateInstance<TClass>() where TClass : class
    {
        return null;
    }

    [Fact]
    public void DTO_To_Entity_IsValid()
    {
        // Arrange
        EntityDTO? entityDTO = CreateInstance<EntityDTO>();

        // Act
        T? entity = entityDTO?.Adapt<T>();

        // Assert
        entityDTO?.ShouldBeSameAs<IType>(entity!);
    }

    [Fact]
    public void Entity_To_VM_IsValid()
    {
        // Arrange
        T? entity = CreateInstance<T>();

        // Act
        EntityVM? entityVM = entity?.Adapt<EntityVM>();

        // Assert
        entity?.ShouldBeSameAs<IType>(entityVM!);
    }

    [Fact]
    public void VM_To_DTO_IsValid()
    {
        // Arrange
        EntityVM? entityVM = CreateInstance<EntityVM>();

        // Act
        EntityDTO? entityDTO = entityVM?.Adapt<EntityDTO>();

        // Assert
        entityVM?.ShouldBeSameAs<IType>(entityDTO!);
    }

    [Fact]
    public void Model_To_Entity_IsValid()
    {
        // Arrange
        EntityModel? entityModel = CreateInstance<EntityModel>();

        // Act
        T? entity = entityModel?.Adapt<T>();

        // Assert
        entityModel?.ShouldBeSameAs<IType>(entity!);
    }

    [Fact]
    public void Entity_To_Model_IsValid()
    {
        // Arrange
        T? entity = CreateInstance<T>();

        // Act
        EntityModel? entityModel = entity?.Adapt<EntityModel>();

        // Assert
        entity?.ShouldBeSameAs<IType>(entityModel!);
    }

    [Fact]
    public void Model_To_DTO_IsValid()
    {
        // Arrange
        EntityModel? entityModel = CreateInstance<EntityModel>();

        // Act
        EntityDTO? entityDTO = entityModel?.Adapt<EntityDTO>();

        // Assert
        entityModel?.ShouldBeSameAs<IType>(entityDTO!);
    }

    [Fact]
    public void DTO_To_Model_IsValid()
    {
        // Arrange
        EntityDTO? entityDTO = CreateInstance<EntityDTO>();

        // Act
        EntityModel? entityModel = entityDTO?.Adapt<EntityModel>();

        // Assert
        entityDTO?.ShouldBeSameAs<IType>(entityModel!);
    }

    [Fact]
    public void VM_To_Model_IsValid()
    {
        // Arrange
        EntityVM? entityVM = CreateInstance<EntityVM>();

        // Act
        EntityModel? entityModel = entityVM?.Adapt<EntityModel>();

        // Assert
        entityVM?.ShouldBeSameAs<IType>(entityModel!);
    }

    [Fact]
    public void Model_To_VM_IsValid()
    {
        // Arrange
        EntityModel? entityModel = CreateInstance<EntityModel>();

        // Act
        EntityVM? entityVM = entityModel?.Adapt<EntityVM>();

        // Assert
        entityModel?.ShouldBeSameAs<IType>(entityVM!);
    }

    [Fact]
    public void VM_To_Entity_IsValid()
    {
        // Arrange
        EntityVM? entityVM = CreateInstance<EntityVM>();

        // Act
        T? entity = entityVM?.Adapt<T>();

        // Assert
        entityVM?.ShouldBeSameAs<IType>(entity!);
    }

    [Fact]
    public void Entity_To_DTO_IsValid()
    {
        // Arrange
        T? entity = CreateInstance<T>();

        // Act
        EntityDTO? entityDTO = entity?.Adapt<EntityDTO>();

        // Assert
        entity?.ShouldBeSameAs<IType>(entityDTO!);
    }

    [Fact]
    public void DTO_To_VM_IsValid()
    {
        // Arrange
        EntityDTO? entityDTO = CreateInstance<EntityDTO>();

        // Act
        EntityVM? entityVM = entityDTO?.Adapt<EntityVM>();

        // Assert
        entityDTO?.ShouldBeSameAs<IType>(entityVM!);
    }
}
