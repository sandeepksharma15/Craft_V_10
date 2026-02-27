using Craft.Auditing;

namespace Craft.AppComponents.Auditing;

public partial class AuditLog
{
    // Filter state
    private string? _filterTable;
    private KeyType? _filterUserId;
    private EntityChangeType? _filterChangeType;
    private DateTime? _filterStartDate;
    private DateTime? _filterEndDate;

    // Grid state
    private List<AuditTrail> _auditTrails = [];
    private bool _isLoading = false;
}
