namespace Craft.TestDataStore.Models;

public static class CountrySeed
{
    public const int COUNTRY_ID_1 = 1;
    public const int COUNTRY_ID_2 = 2;

    public const string COUNTRY_NAME_1 = "Country 1";
    public const string COUNTRY_NAME_2 = "Country 2";

    public static List<Country> Get()
    {
        return
        [
            new() { Id = 1, Name = COUNTRY_NAME_1, },
            new() { Id = 2, Name = COUNTRY_NAME_2, }
        ];
    }
}
