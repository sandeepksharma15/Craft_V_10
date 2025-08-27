using System.ComponentModel.DataAnnotations;

namespace Craft.MultiTenant;

public class MultiTenantOptions : IValidatableObject
{
    public const string SectionName = "MultiTenantOptions";

    public TenantDbType DbType { get; set; } = TenantDbType.PerTenant;
    public bool IsEnabled { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (validationContext.ObjectInstance is MultiTenantOptions options)
        {
            if (options.DbType == TenantDbType.None)
                yield return new ValidationResult("MultiTenant Db Strategy must be specified.", [nameof(options.DbType)]);
        }
    }
}
