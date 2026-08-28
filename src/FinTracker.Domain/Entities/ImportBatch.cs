using FinTracker.Domain.Common;

namespace FinTracker.Domain.Entities;

public class ImportBatch : BaseEntity
{
    public Guid AccountId { get; set; }
    public Account Account { get; set; } = null!;

    public string Filename { get; set; } = string.Empty;
    public string FileType { get; set; } = "ofx";
    public int TotalImported { get; set; } = 0;
    public DateTimeOffset ImportedAt { get; set; } = DateTimeOffset.UtcNow;

    public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
}
