using System.Text.Json.Serialization;
using FinTracker.Domain.Enums;

namespace FinTracker.Api.DTOs;

public record CreateAccountDto(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("institution")] string Institution,
    [property: JsonPropertyName("ownership")] string Ownership,
    [property: JsonPropertyName("initial_balance")] decimal InitialBalance = 0.00m
);

public record UpdateAccountDto(
    [property: JsonPropertyName("name")] string? Name,
    [property: JsonPropertyName("institution")] string? Institution
);

public record AccountResponseDto(
    [property: JsonPropertyName("id")] Guid Id,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("institution")] string Institution,
    [property: JsonPropertyName("ownership")] string Ownership,
    [property: JsonPropertyName("initial_balance")] decimal InitialBalance,
    [property: JsonPropertyName("current_balance")] decimal CurrentBalance,
    [property: JsonPropertyName("currency")] string Currency,
    [property: JsonPropertyName("created_at")] DateTimeOffset CreatedAt,
    [property: JsonPropertyName("updated_at")] DateTimeOffset UpdatedAt
);

public record AccountSummaryDto(
    [property: JsonPropertyName("total_balance")] decimal TotalBalance,
    [property: JsonPropertyName("personal_pf_balance")] decimal PersonalPfBalance,
    [property: JsonPropertyName("business_mei_balance")] decimal BusinessMeiBalance,
    [property: JsonPropertyName("accounts_count")] int AccountsCount
);
