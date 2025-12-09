namespace Craft.Domain;

public interface ISoftDelete
{
    public const string ColumnName = "IsDeleted";

    bool IsDeleted { get; set; }

    public void Delete() => IsDeleted = true;

    public void Restore() => IsDeleted = false;
}
