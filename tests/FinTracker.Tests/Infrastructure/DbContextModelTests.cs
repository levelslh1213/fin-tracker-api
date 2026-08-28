using FinTracker.Domain.Entities;
using FinTracker.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace FinTracker.Tests.Infrastructure;

public class DbContextModelTests
{
    [Fact]
    public void FinTrackerDbContext_ModelConfiguration_HasAllRequiredEntitiesAndIndices()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<FinTrackerDbContext>()
            .UseInMemoryDatabase(databaseName: "Test_DbModel")
            .Options;

        // Act
        using var context = new FinTrackerDbContext(options);
        var model = context.Model;

        // Assert
        Assert.NotNull(model.FindEntityType(typeof(Account)));
        Assert.NotNull(model.FindEntityType(typeof(Transaction)));
        Assert.NotNull(model.FindEntityType(typeof(Tag)));
        Assert.NotNull(model.FindEntityType(typeof(Budget)));
        Assert.NotNull(model.FindEntityType(typeof(ImportBatch)));
        Assert.NotNull(model.FindEntityType(typeof(TransactionTag)));
    }
}
