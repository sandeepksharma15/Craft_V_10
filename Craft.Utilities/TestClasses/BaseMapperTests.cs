using Craft.Utilities.Helpers;
using Mapster;
using Xunit;

namespace Craft.Utilities.TestClasses;

public abstract class BaseMapperTests<T, EntityDTO, EntityVM, IType>
    where T : class, IType
    where EntityDTO : class, IType
    where EntityVM : class, IType
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
        entityDTO?.ShouldBeSameAs<IType>(entity);
    }

    [Fact]
    public void Entity_To_VM_IsValid()
    {
        // Arrange
        T? entity = CreateInstance<T>();

        // Act
        EntityVM? entityVM = entity?.Adapt<EntityVM>();

        // Assert
        entity?.ShouldBeSameAs<IType>(entityVM);
    }

    [Fact]
    public void VM_To_DTO_IsValid()
    {
        // Arrange
        EntityVM? entityVM = CreateInstance<EntityVM>();

        // Act
        EntityDTO? entityDTO = entityVM?.Adapt<EntityDTO>();

        // Assert
        entityVM?.ShouldBeSameAs<IType>(entityDTO);
    }
}
