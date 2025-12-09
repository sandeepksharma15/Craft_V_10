namespace Craft.Testing.Fixtures;

public static class CompanySeed
{
    public const int COMPANY_ID_1 = 1;
    public const string COMPANY_NAME_1 = "Company 1";

    public static List<Company> Get()
    {
        return
        [
            new() { Id = COMPANY_ID_1, Name = COMPANY_NAME_1, CountryId = CountrySeed.COUNTRY_ID_1, },
            new() { Id = 2, Name = "Company 2", CountryId = CountrySeed.COUNTRY_ID_2, },
            new() { Id = 3, Name = "Company 3", CountryId = CountrySeed.COUNTRY_ID_1, }
        ];
    }
}
