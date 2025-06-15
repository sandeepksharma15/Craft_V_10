namespace Craft.Logging.Serilog;

public class CraftSerilogOptions
{
    public SerilogEnricherNames EnricherNames { get; set; } = new SerilogEnricherNames();

    public class SerilogEnricherNames
    {
        public string TenantId { get; set; } = "_TENANT_ID_";
        public string UserId { get; set; } = "_USER_ID_";
        public string ClientId { get; set; } = "_CLIENT_ID_";
    }
}
