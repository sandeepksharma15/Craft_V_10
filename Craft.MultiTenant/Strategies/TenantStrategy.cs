namespace Craft.MultiTenant;

public class TenantStrategy
{
    public ITenantStrategy? Strategy { get; internal set; }

    public Type? StrategyType { get; internal set; }

    public TenantStrategy() { }

    public TenantStrategy(ITenantStrategy? strategy, Type? strategyType)
    {
        Strategy = strategy;
        StrategyType = strategyType;
    }
}
