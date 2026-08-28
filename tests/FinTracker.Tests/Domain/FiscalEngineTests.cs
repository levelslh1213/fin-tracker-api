using FinTracker.Domain.Services;
using Xunit;

namespace FinTracker.Tests.Domain;

public class FiscalEngineTests
{
    [Fact]
    public void CalculateIrpfExemption_WithValidRevenueAndExpenses_CalculatesCorrectNetProfitAndTaxSavings()
    {
        // Arrange: MEI prestador de servi?os fatura R$ 60.000 no ano e tem R$ 15.000 de despesas operacionais
        var input = new IrpfCalculationInput(
            TotalGrossRevenue: 60000.00m,
            TotalOperatingExpenses: 15000.00m,
            PresumptionRate: 0.32m
        );

        // Act
        var result = FiscalEngine.CalculateIrpfExemption(input);

        // Assert
        Assert.Equal(60000.00m, result.TotalGrossRevenue);
        Assert.Equal(15000.00m, result.TotalOperatingExpenses);
        Assert.Equal(45000.00m, result.NetRealProfit); // Lucro Real = 60k - 15k = 45k
        Assert.Equal(19200.00m, result.PresumedExemptLimit); // Presun??o 32% = 19.2k
        Assert.Equal(45000.00m, result.ExemptProfitDistributed); // Com Livro Caixa = 45k 100% Isento
        Assert.Equal(0.00m, result.TaxableProfit); // 0 tribut?vel
        Assert.True(result.TaxSavings > 0); // Economizou imposto
    }
}
