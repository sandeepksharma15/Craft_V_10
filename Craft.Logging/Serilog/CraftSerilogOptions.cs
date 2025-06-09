namespace Craft.Logging.Serilog;

public class CraftSerilogOptions
{
    public SerilogEnricherNames EnricherNames { get; set; } = new SerilogEnricherNames();

    public class SerilogEnricherNames
    {
        public string TenantId { get; set; } = "TenantId";
        public string UserId { get; set; } = "UserId";
        public string ClientId { get; set; } = "ClientId";
    }
}
