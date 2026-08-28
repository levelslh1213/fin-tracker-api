using FinTracker.Domain.Common;

namespace FinTracker.Domain.Entities;

public class Tag : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Color { get; set; } = "#10B981";

    // Navega??es
    public ICollection<TransactionTag> TransactionTags { get; set; } = new List<TransactionTag>();
    public ICollection<Budget> Budgets { get; set; } = new List<Budget>();
}
