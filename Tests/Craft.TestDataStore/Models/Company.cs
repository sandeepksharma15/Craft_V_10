using System.ComponentModel.DataAnnotations.Schema;
using Craft.Domain;

namespace Craft.TestDataStore.Models;

public class Company : BaseEntity
{
    [ForeignKey("CountryId")]
    public Country? Country { get; set; }
    public KeyType CountryId { get; set; }

    public string? Name { get; set; }
    public List<Store>? Stores { get; set; }
}

public class CompanyName
{
    public string? Name { get; set; }
}
