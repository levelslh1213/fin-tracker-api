using FinTracker.Domain.Common;

namespace FinTracker.Domain.Entities;

public class Budget : BaseEntity
{
    public Guid TagId { get; set; }
    public Tag Tag { get; set; } = null!;

    public string Month { get; set; } = string.Empty; // YYYY-MM
    public decimal Amount { get; set; } = 0.00m;
}
