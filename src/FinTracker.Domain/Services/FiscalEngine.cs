namespace FinTracker.Domain.Services;

public record IrpfCalculationInput(
    decimal TotalGrossRevenue,
    decimal TotalOperatingExpenses,
    decimal PresumptionRate = 0.32m
);

public record IrpfCalculationResult(
    decimal TotalGrossRevenue,
    decimal TotalOperatingExpenses,
    decimal NetRealProfit,
    decimal PresumedExemptLimit,
    decimal ExemptProfitDistributed,
    decimal TaxableProfit,
    bool HasAccountingBookkeeping,
    decimal TaxSavings
);

public static class FiscalEngine
{
    /// <summary>
    /// Calcula a apura??o de IRPF para o MEI comparando a escritura??o regular do Livro Caixa vs a presun??o legal (32% para servi?os).
    /// </summary>
    public static IrpfCalculationResult CalculateIrpfExemption(IrpfCalculationInput input)
    {
        var netRealProfit = Math.Max(0.00m, input.TotalGrossRevenue - input.TotalOperatingExpenses);
        var presumedExemptLimit = input.TotalGrossRevenue * input.PresumptionRate;

        // Com o Livro Caixa escriturado, todo o lucro l?quido real ? isento de IRPF
        var exemptProfitWithBookkeeping = netRealProfit;
        var taxableProfitWithBookkeeping = 0.00m;

        // Sem Livro Caixa, a isen??o ? limitada ao percentual de presun??o e o excedente ? tribut?vel
        var taxableProfitWithoutBookkeeping = Math.Max(0.00m, netRealProfit - presumedExemptLimit);
        
        // Estimativa de imposto economizado na faixa progressiva m?dia de 27.5%
        var taxSavings = taxableProfitWithoutBookkeeping * 0.275m;

        return new IrpfCalculationResult(
            TotalGrossRevenue: input.TotalGrossRevenue,
            TotalOperatingExpenses: input.TotalOperatingExpenses,
            NetRealProfit: netRealProfit,
            PresumedExemptLimit: presumedExemptLimit,
            ExemptProfitDistributed: exemptProfitWithBookkeeping,
            TaxableProfit: taxableProfitWithBookkeeping,
            HasAccountingBookkeeping: true,
            TaxSavings: taxSavings
        );
    }
}
