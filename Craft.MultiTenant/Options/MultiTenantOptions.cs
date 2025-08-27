using System.ComponentModel.DataAnnotations;

namespace Craft.MultiTenant;

public class MultiTenantOptions : IValidatableObject
{
    public const string SectionName = "MultiTenantOptions";

    public TenantDbType DbType { get; set; } = TenantDbType.Shared;
    public bool IsEnabled { get; set; } = true;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (validationContext.ObjectInstance is MultiTenantOptions options)
        {
            if (IsEnabled && options.DbType == TenantDbType.None)
                yield return new ValidationResult("MultiTenant Db Strategy must be specified when MultiTenancy is enabled.", [nameof(options.DbType)]);
        }
    }
}
