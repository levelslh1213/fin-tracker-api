using FinTracker.Api.DTOs;
using FinTracker.Domain.Entities;
using FinTracker.Domain.Enums;
using FinTracker.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FinTracker.Api.Endpoints;

public static class AccountEndpoints
{
    public static void MapAccountEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/accounts")
                       .WithTags("Accounts");

        // GET /api/v1/accounts?ownership=all|personal_pf|business_mei
        group.MapGet("/", async (string? ownership, FinTrackerDbContext db) =>
        {
            var query = db.Accounts.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(ownership) && ownership.ToLowerInvariant() != "all")
            {
                if (ownership.Equals("personal_pf", StringComparison.OrdinalIgnoreCase))
                {
                    query = query.Where(a => a.Ownership == AccountOwnership.PersonalPf);
                }
                else if (ownership.Equals("business_mei", StringComparison.OrdinalIgnoreCase))
                {
                    query = query.Where(a => a.Ownership == AccountOwnership.BusinessMei);
                }
            }

            var accounts = await query.OrderBy(a => a.Name).ToListAsync();
            var dtos = accounts.Select(ToDto);

            return Results.Ok(dtos);
        })
        .WithName("ListAccounts")
        .WithSummary("Lista contas financeiras com suporte a filtro por titularidade");

        // GET /api/v1/accounts/summary
        group.MapGet("/summary", async (FinTrackerDbContext db) =>
        {
            var accounts = await db.Accounts.AsNoTracking().ToListAsync();

            var totalBalance = accounts.Sum(a => a.CurrentBalance);
            var pfBalance = accounts.Where(a => a.Ownership == AccountOwnership.PersonalPf).Sum(a => a.CurrentBalance);
            var meiBalance = accounts.Where(a => a.Ownership == AccountOwnership.BusinessMei).Sum(a => a.CurrentBalance);

            var summary = new AccountSummaryDto(
                TotalBalance: totalBalance,
                PersonalPfBalance: pfBalance,
                BusinessMeiBalance: meiBalance,
                AccountsCount: accounts.Count
            );

            return Results.Ok(summary);
        })
        .WithName("GetAccountsSummary")
        .WithSummary("Retorna o resumo de saldos e contagem de contas");

        // GET /api/v1/accounts/{id}
        group.MapGet("/{id:guid}", async (Guid id, FinTrackerDbContext db) =>
        {
            var account = await db.Accounts.FindAsync(id);
            if (account == null)
            {
                return Results.NotFound(new { message = $"Conta com ID {id} não encontrada." });
            }

            return Results.Ok(ToDto(account));
        })
        .WithName("GetAccountById")
        .WithSummary("Busca os detalhes de uma conta financeira por ID");

        // POST /api/v1/accounts
        group.MapPost("/", async (CreateAccountDto dto, FinTrackerDbContext db) =>
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
            {
                return Results.BadRequest(new { message = "O nome da conta é obrigatório." });
            }

            if (string.IsNullOrWhiteSpace(dto.Institution))
            {
                return Results.BadRequest(new { message = "A instituição financeira é obrigatória." });
            }

            var ownership = ParseOwnership(dto.Ownership);

            var account = new Account
            {
                Id = Guid.NewGuid(),
                Name = dto.Name.Trim(),
                Institution = dto.Institution.Trim(),
                Ownership = ownership,
                InitialBalance = dto.InitialBalance,
                CurrentBalance = dto.InitialBalance,
                Currency = "BRL",
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            };

            db.Accounts.Add(account);
            await db.SaveChangesAsync();

            return Results.Created($"/api/v1/accounts/{account.Id}", ToDto(account));
        })
        .WithName("CreateAccount")
        .WithSummary("Cria uma nova conta financeira");

        // PUT /api/v1/accounts/{id}
        group.MapPut("/{id:guid}", async (Guid id, UpdateAccountDto dto, FinTrackerDbContext db) =>
        {
            var account = await db.Accounts.FindAsync(id);
            if (account == null)
            {
                return Results.NotFound(new { message = $"Conta com ID {id} não encontrada." });
            }

            if (!string.IsNullOrWhiteSpace(dto.Name))
            {
                account.Name = dto.Name.Trim();
            }

            if (!string.IsNullOrWhiteSpace(dto.Institution))
            {
                account.Institution = dto.Institution.Trim();
            }

            account.UpdatedAt = DateTimeOffset.UtcNow;
            await db.SaveChangesAsync();

            return Results.Ok(ToDto(account));
        })
        .WithName("UpdateAccount")
        .WithSummary("Atualiza dados cadastrais de uma conta");

        // DELETE /api/v1/accounts/{id}
        group.MapDelete("/{id:guid}", async (Guid id, FinTrackerDbContext db) =>
        {
            var account = await db.Accounts.FindAsync(id);
            if (account == null)
            {
                return Results.NotFound(new { message = $"Conta com ID {id} não encontrada." });
            }

            db.Accounts.Remove(account);
            await db.SaveChangesAsync();

            return Results.NoContent();
        })
        .WithName("DeleteAccount")
        .WithSummary("Remove uma conta financeira");
    }

    private static AccountOwnership ParseOwnership(string? ownership)
    {
        if (string.IsNullOrWhiteSpace(ownership)) return AccountOwnership.PersonalPf;

        return ownership.ToLowerInvariant() switch
        {
            "business_mei" or "mei" or "pj" => AccountOwnership.BusinessMei,
            _ => AccountOwnership.PersonalPf
        };
    }

    private static string FormatOwnership(AccountOwnership ownership) =>
        ownership switch
        {
            AccountOwnership.BusinessMei => "business_mei",
            _ => "personal_pf"
        };

    private static AccountResponseDto ToDto(Account a) =>
        new(
            Id: a.Id,
            Name: a.Name,
            Institution: a.Institution,
            Ownership: FormatOwnership(a.Ownership),
            InitialBalance: a.InitialBalance,
            CurrentBalance: a.CurrentBalance,
            Currency: a.Currency,
            CreatedAt: a.CreatedAt,
            UpdatedAt: a.UpdatedAt
        );
}
