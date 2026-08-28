using FinTracker.Domain.Common;
using FinTracker.Domain.Enums;

namespace FinTracker.Domain.Entities;

public class Transaction : BaseEntity
{
    public Guid AccountId { get; set; }
    public Account Account { get; set; } = null!;

    public Guid? ImportBatchId { get; set; }
    public ImportBatch? ImportBatch { get; set; }

    public DateOnly Date { get; set; }
    public decimal Amount { get; set; }
    public TransactionType Type { get; set; } = TransactionType.Expense;
    public TransactionStatus Status { get; set; } = TransactionStatus.Cleared;
    public bool IsRecurring { get; set; } = false;
    public RecurrencePeriod? RecurrencePeriod { get; set; }

    public string Description { get; set; } = string.Empty;
    public string? InvoiceNumber { get; set; }
    public string Fingerprint { get; set; } = string.Empty;

    // Navega??es
    public ICollection<TransactionTag> TransactionTags { get; set; } = new List<TransactionTag>();
}
