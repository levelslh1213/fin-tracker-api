using FinTracker.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FinTracker.Infrastructure.Data;

public class FinTrackerDbContext : DbContext
{
    public FinTrackerDbContext(DbContextOptions<FinTrackerDbContext> options) : base(options)
    {
    }

    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<Transaction> Transactions => Set<Transaction>();
    public DbSet<Tag> Tags => Set<Tag>();
    public DbSet<TransactionTag> TransactionTags => Set<TransactionTag>();
    public DbSet<Budget> Budgets => Set<Budget>();
    public DbSet<ImportBatch> ImportBatches => Set<ImportBatch>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Account
        modelBuilder.Entity<Account>(entity =>
        {
            entity.ToTable("accounts");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Institution).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Ownership).HasConversion<string>().HasMaxLength(20).IsRequired();
            entity.Property(e => e.InitialBalance).HasPrecision(12, 2).IsRequired();
            entity.Property(e => e.CurrentBalance).HasPrecision(12, 2).IsRequired();
            entity.Property(e => e.Currency).HasMaxLength(3).HasDefaultValue("BRL").IsRequired();

            entity.HasIndex(e => e.Ownership).HasDatabaseName("idx_accounts_ownership");
        });

        // Transaction
        modelBuilder.Entity<Transaction>(entity =>
        {
            entity.ToTable("transactions");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Date).IsRequired();
            entity.Property(e => e.Amount).HasPrecision(12, 2).IsRequired();
            entity.Property(e => e.Type).HasConversion<string>().HasMaxLength(10).IsRequired();
            entity.Property(e => e.Status).HasConversion<string>().HasMaxLength(10).IsRequired();
            entity.Property(e => e.RecurrencePeriod).HasConversion<string>().HasMaxLength(10);
            entity.Property(e => e.Description).HasMaxLength(255).IsRequired();
            entity.Property(e => e.InvoiceNumber).HasMaxLength(50);
            entity.Property(e => e.Fingerprint).HasMaxLength(64).IsRequired();

            entity.HasOne(e => e.Account)
                  .WithMany(a => a.Transactions)
                  .HasForeignKey(e => e.AccountId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.ImportBatch)
                  .WithMany(b => b.Transactions)
                  .HasForeignKey(e => e.ImportBatchId)
                  .OnDelete(DeleteBehavior.SetNull);

            entity.HasIndex(e => new { e.AccountId, e.Date }).HasDatabaseName("idx_transactions_acc_date");
            entity.HasIndex(e => e.Fingerprint).HasDatabaseName("idx_transactions_fingerprint");
            entity.HasIndex(e => e.Date).HasDatabaseName("idx_transactions_date");
        });

        // Tag
        modelBuilder.Entity<Tag>(entity =>
        {
            entity.ToTable("tags");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Color).HasMaxLength(7).IsRequired();

            entity.HasIndex(e => e.Name).IsUnique().HasDatabaseName("uk_tags_name");
        });

        // TransactionTag (N:N)
        modelBuilder.Entity<TransactionTag>(entity =>
        {
            entity.ToTable("transaction_tags");
            entity.HasKey(e => new { e.TransactionId, e.TagId });

            entity.HasOne(e => e.Transaction)
                  .WithMany(t => t.TransactionTags)
                  .HasForeignKey(e => e.TransactionId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Tag)
                  .WithMany(t => t.TransactionTags)
                  .HasForeignKey(e => e.TagId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Budget
        modelBuilder.Entity<Budget>(entity =>
        {
            entity.ToTable("budgets");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Month).HasMaxLength(7).IsRequired();
            entity.Property(e => e.Amount).HasPrecision(12, 2).IsRequired();

            entity.HasOne(e => e.Tag)
                  .WithMany(t => t.Budgets)
                  .HasForeignKey(e => e.TagId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => new { e.TagId, e.Month }).IsUnique().HasDatabaseName("uk_budgets_tag_month");
        });

        // ImportBatch
        modelBuilder.Entity<ImportBatch>(entity =>
        {
            entity.ToTable("import_batches");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Filename).HasMaxLength(255).IsRequired();
            entity.Property(e => e.FileType).HasMaxLength(10).IsRequired();

            entity.HasOne(e => e.Account)
                  .WithMany(a => a.ImportBatches)
                  .HasForeignKey(e => e.AccountId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => new { e.AccountId, e.ImportedAt }).HasDatabaseName("idx_import_batches_account");
        });
    }
}
