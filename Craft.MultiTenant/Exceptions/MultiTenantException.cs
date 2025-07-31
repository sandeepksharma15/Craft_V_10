namespace Craft.MultiTenant;

public class MultiTenantException : Exception
{
    public MultiTenantException(string message) : base(message) { }

    public MultiTenantException(string message, Exception innerException)
        : base(message, innerException) { }

    public MultiTenantException() { }
}
