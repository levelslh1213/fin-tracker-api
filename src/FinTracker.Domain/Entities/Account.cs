using FinTracker.Domain.Common;
using FinTracker.Domain.Enums;

namespace FinTracker.Domain.Entities;

public class Account : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Institution { get; set; } = string.Empty;
    public AccountOwnership Ownership { get; set; } = AccountOwnership.PersonalPf;
    public decimal InitialBalance { get; set; } = 0.00m;
    public decimal CurrentBalance { get; set; } = 0.00m;
    public string Currency { get; set; } = "BRL";

    // Navega??es
    public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
    public ICollection<ImportBatch> ImportBatches { get; set; } = new List<ImportBatch>();
}
