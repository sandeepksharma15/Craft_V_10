namespace Craft.Domain;

public interface IHasUser<TKey>
{
    public const string ColumnName = "UserId";

    TKey UserId { get; set; }

    TKey GetUserId() => UserId;

    bool IsUserIdSet() => !(UserId!.Equals(default(TKey)));

    void SetUserId(TKey userId) => UserId = userId;
}

public interface IHasUser : IHasUser<KeyType>;
