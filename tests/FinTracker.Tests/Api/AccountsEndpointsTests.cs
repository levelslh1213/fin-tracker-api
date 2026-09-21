using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FinTracker.Tests.Fixtures;
using FluentAssertions;
using Xunit;

namespace FinTracker.Tests.Api;

public class AccountsEndpointsTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public AccountsEndpointsTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task CreateAccount_ValidPayload_Returns201Created_AndCalculatesInitialBalance()
    {
        // Arrange
        var newAccount = new
        {
            name = "Inter MEI",
            institution = "Banco Inter",
            ownership = "business_mei",
            initial_balance = 2500.50m
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/accounts", newAccount);
        var body = await response.Content.ReadAsStringAsync();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created, because: body);
        var created = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        
        created.GetProperty("id").GetString().Should().NotBeNullOrWhiteSpace();
        created.GetProperty("name").GetString().Should().Be("Inter MEI");
        created.GetProperty("institution").GetString().Should().Be("Banco Inter");
        created.GetProperty("ownership").GetString().Should().Be("business_mei");
        created.GetProperty("initial_balance").GetDecimal().Should().Be(2500.50m);
        created.GetProperty("current_balance").GetDecimal().Should().Be(2500.50m);
    }

    [Fact]
    public async Task CreateAccount_MissingName_Returns400BadRequest()
    {
        // Arrange
        var invalidAccount = new
        {
            name = "",
            institution = "Banco Inter",
            ownership = "personal_pf",
            initial_balance = 100.00m
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/accounts", invalidAccount);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ListAccounts_FilterByOwnership_ReturnsFilteredResults()
    {
        // Arrange - Cria uma conta PF e uma conta MEI
        var pfAccount = new
        {
            name = "Nubank PF",
            institution = "Nubank",
            ownership = "personal_pf",
            initial_balance = 1000.00m
        };
        var meiAccount = new
        {
            name = "Inter PJ",
            institution = "Banco Inter",
            ownership = "business_mei",
            initial_balance = 5000.00m
        };

        await _client.PostAsJsonAsync("/api/v1/accounts", pfAccount);
        await _client.PostAsJsonAsync("/api/v1/accounts", meiAccount);

        // Act & Assert - Filtro PF
        var pfResponse = await _client.GetAsync("/api/v1/accounts?ownership=personal_pf");
        pfResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var pfList = await pfResponse.Content.ReadFromJsonAsync<JsonElement[]>(JsonOptions);
        pfList.Should().NotBeNull();
        pfList!.All(a => a.GetProperty("ownership").GetString() == "personal_pf").Should().BeTrue();

        // Act & Assert - Filtro MEI
        var meiResponse = await _client.GetAsync("/api/v1/accounts?ownership=business_mei");
        meiResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var meiList = await meiResponse.Content.ReadFromJsonAsync<JsonElement[]>(JsonOptions);
        meiList.Should().NotBeNull();
        meiList!.All(a => a.GetProperty("ownership").GetString() == "business_mei").Should().BeTrue();

        // Act & Assert - Filtro All
        var allResponse = await _client.GetAsync("/api/v1/accounts?ownership=all");
        allResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var allList = await allResponse.Content.ReadFromJsonAsync<JsonElement[]>(JsonOptions);
        allList.Should().NotBeNull();
        allList!.Length.Should().BeGreaterThanOrEqualTo(2);
    }

    [Fact]
    public async Task GetAccountById_ExistingId_Returns200Ok()
    {
        // Arrange
        var account = new
        {
            name = "Caixa Econômica PF",
            institution = "Caixa",
            ownership = "personal_pf",
            initial_balance = 300.00m
        };
        var createResponse = await _client.PostAsJsonAsync("/api/v1/accounts", account);
        var created = await createResponse.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        var id = created.GetProperty("id").GetString();

        // Act
        var getResponse = await _client.GetAsync($"/api/v1/accounts/{id}");

        // Assert
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var fetched = await getResponse.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        fetched.GetProperty("id").GetString().Should().Be(id);
        fetched.GetProperty("name").GetString().Should().Be("Caixa Econômica PF");
    }

    [Fact]
    public async Task GetAccountById_NonExistingId_Returns404NotFound()
    {
        // Act
        var response = await _client.GetAsync($"/api/v1/accounts/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateAccount_ValidUpdate_Returns200Ok()
    {
        // Arrange
        var account = new
        {
            name = "Conta Antiga",
            institution = "Banco X",
            ownership = "personal_pf",
            initial_balance = 100.00m
        };
        var createResponse = await _client.PostAsJsonAsync("/api/v1/accounts", account);
        var created = await createResponse.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        var id = created.GetProperty("id").GetString();

        var updatePayload = new
        {
            name = "Conta Nova Atualizada",
            institution = "Banco Y"
        };

        // Act
        var putResponse = await _client.PutAsJsonAsync($"/api/v1/accounts/{id}", updatePayload);

        // Assert
        putResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var updated = await putResponse.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        updated.GetProperty("name").GetString().Should().Be("Conta Nova Atualizada");
        updated.GetProperty("institution").GetString().Should().Be("Banco Y");
    }

    [Fact]
    public async Task DeleteAccount_ExistingId_Returns204NoContent_AndSubsequentGetReturns404()
    {
        // Arrange
        var account = new
        {
            name = "Conta Para Deletar",
            institution = "Banco Z",
            ownership = "personal_pf",
            initial_balance = 0.00m
        };
        var createResponse = await _client.PostAsJsonAsync("/api/v1/accounts", account);
        var created = await createResponse.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        var id = created.GetProperty("id").GetString();

        // Act
        var deleteResponse = await _client.DeleteAsync($"/api/v1/accounts/{id}");

        // Assert
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var getResponse = await _client.GetAsync($"/api/v1/accounts/{id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetAccountsSummary_ReturnsAggregatedBalances()
    {
        // Act
        var summaryResponse = await _client.GetAsync("/api/v1/accounts/summary");

        // Assert
        summaryResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var summary = await summaryResponse.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        summary.TryGetProperty("total_balance", out _).Should().BeTrue();
        summary.TryGetProperty("personal_pf_balance", out _).Should().BeTrue();
        summary.TryGetProperty("business_mei_balance", out _).Should().BeTrue();
        summary.TryGetProperty("accounts_count", out _).Should().BeTrue();
    }
}
