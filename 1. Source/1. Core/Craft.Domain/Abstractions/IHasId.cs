namespace Craft.Domain;

public interface IHasId<TKey>
{
    public const string ColumnName = "Id";

    TKey Id { get; set; }

    bool IsNew => Id!.Equals(default(TKey));

    TKey GetId() => Id;

    void SetId(TKey id) => Id = id;
}

public interface IHasId : IHasId<KeyType>;
