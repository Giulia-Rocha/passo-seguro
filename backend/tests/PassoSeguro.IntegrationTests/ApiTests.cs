using System.Diagnostics;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Npgsql;
using PassoSeguro.Application;
using PassoSeguro.Infrastructure;
namespace PassoSeguro.IntegrationTests;

public sealed class ApiFixture : IAsyncLifetime
{
    private readonly string container = "passoseguro-tests-" + Guid.NewGuid().ToString("N");
    public readonly string Password = Guid.NewGuid().ToString("N") + "Aa1!";
    public WebApplicationFactory<Program> Factory { get; private set; } = null!;
    public async Task InitializeAsync()
    {
        await Docker("run", "-d", "--rm", "--name", container, "-e", "POSTGRES_PASSWORD=" + Password, "-e", "POSTGRES_DB=passoseguro_tests", "-p", "127.0.0.1::5432", "postgres:16-alpine");
        var port = (await Docker("port", container, "5432/tcp")).Trim().Split(':')[^1];
        var connection = $"Host=127.0.0.1;Port={port};Database=passoseguro_tests;Username=postgres;Password={Password};Include Error Detail=true";
        for (var i = 0; i < 60; i++) { try { await using var conn = new NpgsqlConnection(connection); await conn.OpenAsync(); break; } catch when (i < 59) { await Task.Delay(500); } }
        Environment.SetEnvironmentVariable("Jwt__Key", Guid.NewGuid().ToString("N") + Guid.NewGuid().ToString("N"));
        Environment.SetEnvironmentVariable("ConnectionStrings__Database", connection);
        Factory = new WebApplicationFactory<Program>();
        using var scope = Factory.Services.CreateScope(); var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.MigrateAsync(); await DemoSeed.Apply(db, Password);
        await DemoSeed.Apply(db, Password);
        Assert.Equal(15, await db.Occurrences.CountAsync());
        Assert.All(await db.Occurrences.Include(x => x.History).ToListAsync(), occurrence =>
            Assert.All(occurrence.History, entry => Assert.True(entry.CreatedAt >= occurrence.CreatedAt)));
        Assert.True((await db.Database.GetAppliedMigrationsAsync()).Count() >= 2);
    }
    public async Task DisposeAsync() { if (Factory != null) await Factory.DisposeAsync(); await Docker("stop", container); }
    private static async Task<string> Docker(params string[] arguments)
    {
        var info = new ProcessStartInfo("docker") { RedirectStandardOutput = true, RedirectStandardError = true, CreateNoWindow = true, UseShellExecute = false };
        foreach (var arg in arguments) info.ArgumentList.Add(arg);
        using var process = Process.Start(info)!; var output = process.StandardOutput.ReadToEndAsync(); var error = process.StandardError.ReadToEndAsync(); await process.WaitForExitAsync();
        if (process.ExitCode != 0) throw new InvalidOperationException("Docker indisponível: " + await error); return await output;
    }
    public async Task<HttpClient> Citizen()
    {
        var client = Factory.CreateClient(); var response = await client.PostAsJsonAsync("/api/v1/auth/register", new { name = "Pessoa fictícia", email = Guid.NewGuid() + "@example.test", password = Password });
        response.EnsureSuccessStatusCode(); var value = await response.Content.ReadFromJsonAsync<AuthResponse>(); client.DefaultRequestHeaders.Authorization = new("Bearer", value!.AccessToken); return client;
    }
}
public sealed class ApiTests(ApiFixture fixture) : IClassFixture<ApiFixture>
{
    [Fact]
    public async Task OpenApiDocumentsActualSuccessCodesAndNoContentBodies()
    {
        using var client = await fixture.Citizen();
        var document = await client.GetFromJsonAsync<JsonElement>("/swagger/v1/swagger.json");
        var paths = document.GetProperty("paths");
        var expected = new[]
        {
            ("/api/v1/occurrences", "post", "201"),
            ("/api/v1/occurrences/{id}", "delete", "204"),
            ("/api/v1/occurrences/{id}/photo", "put", "204"),
            ("/api/v1/occurrences/{id}/photo", "delete", "204"),
            ("/api/v1/auth/logout", "post", "204")
        };
        foreach (var (path, method, code) in expected)
        {
            var responses = paths.GetProperty(path).GetProperty(method).GetProperty("responses");
            Assert.True(responses.TryGetProperty(code, out var response), $"Missing {code} for {method} {path}");
            Assert.False(responses.TryGetProperty("200", out _), $"Incorrect 200 for {method} {path}");
            if (code == "204") Assert.False(response.TryGetProperty("content", out _));
        }

        var created = await Create(client, Guid.NewGuid());
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        Assert.NotNull(created.Headers.Location);
        var id = (await created.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id").GetGuid();
        var deleted = await client.DeleteAsync($"/api/v1/occurrences/{id}");
        Assert.Equal(HttpStatusCode.NoContent, deleted.StatusCode);
        Assert.Empty(await deleted.Content.ReadAsByteArrayAsync());
    }

    [Fact]
    public async Task OpenApiDocumentsModelsBinaryPhotosErrorsAndAuthentication()
    {
        using var client = fixture.Factory.CreateClient();
        var document = await client.GetFromJsonAsync<JsonElement>("/swagger/v1/swagger.json");
        var paths = document.GetProperty("paths");
        foreach (var path in paths.EnumerateObject())
            foreach (var operation in path.Value.EnumerateObject())
            {
                Assert.False(string.IsNullOrWhiteSpace(operation.Value.GetProperty("summary").GetString()));
                Assert.True(operation.Value.GetProperty("tags").GetArrayLength() > 0);
            }

        var schemas = document.GetProperty("components").GetProperty("schemas");
        foreach (var name in new[] { "AuthResponse", "UserResponse", "OccurrenceRequest", "OccurrenceResponse", "OccurrenceResponsePagedResponse", "IndicatorsResponse", "CountResponse", "ConcentrationResponse", "PriorityResult", "HistoryResponse", "OccurrenceStatus", "HealthResponse", "ProblemDetails", "ValidationProblemDetails" })
            Assert.True(schemas.GetProperty(name).TryGetProperty("example", out _), $"Missing example for {name}");
        Assert.True(schemas.GetProperty("OccurrenceRequest").GetProperty("example").TryGetProperty("address", out _));
        var list = paths.GetProperty("/api/v1/occurrences").GetProperty("get");
        Assert.True(list.GetProperty("security")[0].TryGetProperty("Bearer", out _));
        Assert.False(list.GetProperty("responses").TryGetProperty("429", out _));
        var login = paths.GetProperty("/api/v1/auth/login").GetProperty("post");
        Assert.True(login.GetProperty("responses").TryGetProperty("429", out _));
        var photo = paths.GetProperty("/api/v1/occurrences/{id}/photo").GetProperty("get").GetProperty("responses").GetProperty("200").GetProperty("content");
        foreach (var mediaType in new[] { "image/png", "image/jpeg" })
            Assert.Equal("binary", photo.GetProperty(mediaType).GetProperty("schema").GetProperty("format").GetString());
        var validation = list.GetProperty("responses").GetProperty("400").GetProperty("content").GetProperty("application/problem+json");
        Assert.True(validation.GetProperty("example").TryGetProperty("errors", out _));
        var domain = list.GetProperty("responses").GetProperty("422").GetProperty("content").GetProperty("application/problem+json");
        Assert.Equal(422, domain.GetProperty("example").GetProperty("status").GetInt32());
        var territory = paths.GetProperty("/api/v1/territory").GetProperty("get").GetProperty("responses").GetProperty("200").GetProperty("content").GetProperty("application/json");
        Assert.Equal("FeatureCollection", territory.GetProperty("example").GetProperty("type").GetString());
    }

    [Fact]
    public async Task HealthUnavailableMatchesTheDocumented503Response()
    {
        using var unavailable = fixture.Factory.WithWebHostBuilder(builder => builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<AppDbContext>>();
            services.AddDbContext<AppDbContext>(options => options.UseNpgsql("Host=127.0.0.1;Port=1;Database=unavailable;Username=unavailable;Password=unused-test-value;Timeout=1;Pooling=false"));
        }));
        using var client = unavailable.CreateClient();
        var response = await client.GetAsync("/api/v1/health");
        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("unavailable", body.GetProperty("status").GetString());
        var document = await client.GetFromJsonAsync<JsonElement>("/swagger/v1/swagger.json");
        var health = document.GetProperty("paths").GetProperty("/api/v1/health").GetProperty("get");
        Assert.False(health.TryGetProperty("security", out _));
        var example = health.GetProperty("responses").GetProperty("503").GetProperty("content").GetProperty("application/json").GetProperty("example");
        Assert.Equal("unavailable", example.GetProperty("status").GetString());
        Assert.False(example.TryGetProperty("database", out _));
    }
    [Fact]
    public async Task AddressPersistsOnCreateEditAndDetail()
    {
        using var client = await fixture.Citizen();
        var input = new OccurrenceRequest(1, "Obstáculo fictício para testar endereço", 3, -23.557, -46.646, "Rua Treze de Maio, 100");
        var created = await Create(client, Guid.NewGuid(), input);
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        var body = await created.Content.ReadFromJsonAsync<JsonElement>();
        var id = body.GetProperty("id").GetGuid();
        Assert.Equal(input.Address, body.GetProperty("address").GetString());
        var edited = await client.PutAsJsonAsync($"/api/v1/occurrences/{id}", input with { Address = "Rua Treze de Maio, 102" });
        Assert.Equal(HttpStatusCode.OK, edited.StatusCode);
        var detail = await client.GetFromJsonAsync<JsonElement>($"/api/v1/occurrences/{id}");
        Assert.Equal("Rua Treze de Maio, 102", detail.GetProperty("address").GetString());
        using var scope = fixture.Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        Assert.Equal("Rua Treze de Maio, 102", (await db.Occurrences.AsNoTracking().SingleAsync(x => x.Id == id)).Address);
    }
    private static object Input(int difficulty = 3, double latitude = -23.557) => new { categoryId = 1, description = "Obstáculo de teste na passagem", difficulty, latitude, longitude = -46.646 };
    private static async Task<HttpResponseMessage> Create(HttpClient client, Guid key, object? input = null)
    { using var message = new HttpRequestMessage(HttpMethod.Post, "/api/v1/occurrences") { Content = JsonContent.Create(input ?? Input()) }; message.Headers.Add("Idempotency-Key", key.ToString()); return await client.SendAsync(message); }
    [Fact] public async Task RequiresAuthentication() { using var client = fixture.Factory.CreateClient(); Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/api/v1/occurrences")).StatusCode); }
    [Fact]
    public async Task CrudActuallyPersistsAndReturnsContract()
    {
        using var client = await fixture.Citizen(); var created = await Create(client, Guid.NewGuid()); Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        var body = await created.Content.ReadFromJsonAsync<JsonElement>(); var id = body.GetProperty("id").GetGuid(); Assert.Equal("Média", body.GetProperty("priority").GetProperty("level").GetString());
        var detail = await client.GetAsync($"/api/v1/occurrences/{id}"); Assert.Equal(HttpStatusCode.OK, detail.StatusCode);
        var edit = await client.PutAsJsonAsync($"/api/v1/occurrences/{id}", Input(5)); Assert.Equal(HttpStatusCode.OK, edit.StatusCode);
        using (var scope = fixture.Factory.Services.CreateScope()) { var db = scope.ServiceProvider.GetRequiredService<AppDbContext>(); Assert.Equal(5, (await db.Occurrences.AsNoTracking().SingleAsync(x => x.Id == id)).Difficulty); }
        var filter = await client.GetFromJsonAsync<JsonElement>("/api/v1/occurrences?mine=true&difficulty=5&pageSize=1"); Assert.Equal(1, filter.GetProperty("total").GetInt32());
        Assert.Equal(HttpStatusCode.NoContent, (await client.DeleteAsync($"/api/v1/occurrences/{id}")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync($"/api/v1/occurrences/{id}")).StatusCode);
    }
    [Fact]
    public async Task DuplicateRequestReturnsSameOccurrenceAndRejectsChangedPayload()
    {
        using var client = await fixture.Citizen(); var key = Guid.NewGuid(); var first = await Create(client, key); var second = await Create(client, key);
        Assert.Equal((await first.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id").GetString(), (await second.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id").GetString());
        Assert.Equal(HttpStatusCode.Conflict, (await Create(client, key, Input(5))).StatusCode);
    }
    [Fact]
    public async Task RejectsOutsideTerritoryAndInvalidDifficulty()
    {
        using var client = await fixture.Citizen(); var outside = await Create(client, Guid.NewGuid(), Input(latitude: 0)); Assert.Equal(HttpStatusCode.UnprocessableEntity, outside.StatusCode);
        var body = await outside.Content.ReadFromJsonAsync<JsonElement>(); Assert.Contains("Bela Vista", body.GetProperty("title").GetString());
        Assert.Equal(HttpStatusCode.BadRequest, (await Create(client, Guid.NewGuid(), Input(6))).StatusCode);
    }
    [Fact]
    public async Task EnforcesOwnershipAndManagerTransitions()
    {
        using var author = await fixture.Citizen(); using var other = await fixture.Citizen(); var response = await Create(author, Guid.NewGuid()); var id = (await response.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id").GetGuid();
        Assert.Equal(HttpStatusCode.Forbidden, (await other.DeleteAsync($"/api/v1/occurrences/{id}")).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await author.PatchAsJsonAsync($"/api/v1/occurrences/{id}/status", new { status = "InReview" })).StatusCode);
        using var manager = fixture.Factory.CreateClient(); var login = await manager.PostAsJsonAsync("/api/v1/auth/login", new { email = "gestor@passoseguro.test", password = fixture.Password }); var auth = await login.Content.ReadFromJsonAsync<AuthResponse>(); manager.DefaultRequestHeaders.Authorization = new("Bearer", auth!.AccessToken);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, (await manager.PatchAsJsonAsync($"/api/v1/occurrences/{id}/status", new { status = "Resolved" })).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await manager.PatchAsJsonAsync($"/api/v1/occurrences/{id}/status", new { status = "InReview" })).StatusCode);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, (await author.PutAsJsonAsync($"/api/v1/occurrences/{id}", Input())).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await manager.PatchAsJsonAsync($"/api/v1/occurrences/{id}/status", new { status = "Resolved" })).StatusCode);
        var history = await manager.GetFromJsonAsync<JsonElement>($"/api/v1/occurrences/{id}/history"); Assert.Equal(3, history.GetArrayLength());
    }
    [Fact]
    public async Task RefreshIsRotatedAndLogoutRevokes()
    {
        using var client = fixture.Factory.CreateClient(); var response = await client.PostAsJsonAsync("/api/v1/auth/login", new { email = "cidadao@passoseguro.test", password = fixture.Password }); var auth = (await response.Content.ReadFromJsonAsync<AuthResponse>())!;
        var refresh = await client.PostAsJsonAsync("/api/v1/auth/refresh", new { refreshToken = auth.RefreshToken }); Assert.Equal(HttpStatusCode.OK, refresh.StatusCode); var next = (await refresh.Content.ReadFromJsonAsync<AuthResponse>())!;
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.PostAsJsonAsync("/api/v1/auth/refresh", new { refreshToken = auth.RefreshToken })).StatusCode);
        await client.PostAsJsonAsync("/api/v1/auth/logout", new { refreshToken = next.RefreshToken }); Assert.Equal(HttpStatusCode.Unauthorized, (await client.PostAsJsonAsync("/api/v1/auth/refresh", new { refreshToken = next.RefreshToken })).StatusCode);
    }
    [Fact]
    public async Task IndicatorsHealthAndOpenApiAreAvailable()
    {
        using var client = await fixture.Citizen(); var health = await client.GetFromJsonAsync<JsonElement>("/api/v1/health"); Assert.Equal("healthy", health.GetProperty("status").GetString());
        var indicators = await client.GetFromJsonAsync<JsonElement>("/api/v1/indicators"); Assert.True(indicators.GetProperty("total").GetInt32() >= 15);
        var swagger = await client.GetFromJsonAsync<JsonElement>("/swagger/v1/swagger.json"); Assert.True(swagger.GetProperty("paths").EnumerateObject().Count() >= 10);
    }
    [Fact]
    public async Task PhotoUploadPersistsAndRejectsInvalidContent()
    {
        using var client = await fixture.Citizen(); var response = await Create(client, Guid.NewGuid()); var id = (await response.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id").GetGuid();
        using var invalid = new MultipartFormDataContent(); invalid.Add(new ByteArrayContent([1, 2, 3]), "file", "bad.png"); Assert.Equal(HttpStatusCode.UnsupportedMediaType, (await client.PutAsync($"/api/v1/occurrences/{id}/photo", invalid)).StatusCode);
        var png = Convert.FromBase64String("iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mP8/x8AAwMCAO+a9V8AAAAASUVORK5CYII=");
        using var form = new MultipartFormDataContent(); form.Add(new ByteArrayContent(png), "file", "test.png"); Assert.Equal(HttpStatusCode.NoContent, (await client.PutAsync($"/api/v1/occurrences/{id}/photo", form)).StatusCode);
        Assert.Equal(png, await client.GetByteArrayAsync($"/api/v1/occurrences/{id}/photo")); Assert.Equal(HttpStatusCode.NoContent, (await client.DeleteAsync($"/api/v1/occurrences/{id}/photo")).StatusCode);
    }
}
