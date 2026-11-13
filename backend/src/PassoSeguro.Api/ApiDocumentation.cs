using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.OpenApi;
using PassoSeguro.Application;
using PassoSeguro.Domain;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace PassoSeguro.Api;

public sealed class ApiDocumentation(ITerritory territory) : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var method = context.MethodInfo;
        bool Has<T>() where T : Attribute => method.IsDefined(typeof(T), true) || method.DeclaringType!.IsDefined(typeof(T), true);
        var responses = operation.Responses!;
        if (!Has<AllowAnonymousAttribute>() && Has<AuthorizeAttribute>())
        {
            operation.Security = [new OpenApiSecurityRequirement { [new OpenApiSecuritySchemeReference("Bearer", context.Document)] = [] }];
            responses.TryAdd("401", new OpenApiResponse { Description = "Sessão ausente ou expirada (sem corpo)." });
            responses.TryAdd("403", new OpenApiResponse { Description = "Operação sem permissão (sem corpo)." });
        }
        if (Has<EnableRateLimitingAttribute>())
            responses.TryAdd("429", new OpenApiResponse { Description = "Limite de tentativas excedido (sem corpo)." });

        if (context.ApiDescription.ParameterDescriptions.Count > 0)
            responses.TryAdd("400", new OpenApiResponse
            {
                Description = "Entrada inválida.",
                Content = new Dictionary<string, OpenApiMediaType>
                {
                    ["application/problem+json"] = new() { Schema = context.SchemaGenerator.GenerateSchema(typeof(ValidationProblemDetails), context.SchemaRepository), Example = ApiExamples.For(typeof(ValidationProblemDetails)) }
                }
            });

        responses.TryAdd("500", new OpenApiResponse { Description = "Não foi possível concluir a operação." });
        foreach (var (code, response) in responses)
        {
            if (response is not OpenApiResponse concrete) continue;
            if (code == "204") { concrete.Description = "Operação concluída, sem corpo de resposta."; concrete.Content?.Clear(); continue; }
            if (code is "404" or "409" or "413" or "415" or "422" or "500")
            {
                concrete.Description = code switch { "404" => "Registro não encontrado.", "409" => "Conflito de atualização ou chave de envio.", "413" => "Foto acima do limite de tamanho.", "415" => "Formato de imagem não aceito.", "422" => "Regra de negócio violada.", _ => "Não foi possível concluir a operação." };
                concrete.Content = new Dictionary<string, OpenApiMediaType>
                {
                    ["application/problem+json"] = new()
                    {
                        Schema = context.SchemaGenerator.GenerateSchema(typeof(ProblemDetails), context.SchemaRepository),
                        Example = JsonSerializer.SerializeToNode(new { status = int.Parse(code), title = concrete.Description })
                    }
                };
            }
        }

        if (method.Name == nameof(OccurrencesController.GetPhoto))
            ((OpenApiResponse)responses["200"]).Content = new Dictionary<string, OpenApiMediaType>
            {
                ["image/jpeg"] = new() { Schema = new OpenApiSchema { Type = JsonSchemaType.String, Format = "binary" } },
                ["image/png"] = new() { Schema = new OpenApiSchema { Type = JsonSchemaType.String, Format = "binary" } }
            };

        if (method.Name == nameof(QueriesController.Territory))
            foreach (var media in responses["200"].Content!.Values)
                media.Example = JsonSerializer.SerializeToNode(territory.GeoJson);

        if (method.Name == nameof(QueriesController.Health))
        {
            foreach (var media in responses["503"].Content!.Values)
                media.Example = JsonNode.Parse("""{"status":"unavailable"}""");
        }
        if (method.Name == nameof(OccurrencesController.Create))
        {
            foreach (var parameter in operation.Parameters!.OfType<OpenApiParameter>().Where(x => x.Name == "Idempotency-Key"))
            {
                parameter.Required = true;
                parameter.Description = "UUID obrigatório. Repita a mesma chave e conteúdo para recuperar um envio; conteúdo diferente com a mesma chave retorna 409.";
                parameter.Example = JsonValue.Create("33333333-3333-3333-3333-333333333333");
            }
            ((OpenApiResponse)responses["201"]).Headers = new Dictionary<string, IOpenApiHeader>
            {
                ["Location"] = new OpenApiHeader { Description = "URL do detalhe da ocorrência criada.", Schema = new OpenApiSchema { Type = JsonSchemaType.String, Format = "uri-reference" } }
            };
        }
    }
}

public sealed class ApiExamples : ISchemaFilter
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };
    private static readonly DateTimeOffset CreatedAt = new(2026, 11, 13, 12, 0, 0, TimeSpan.Zero);
    private static UserResponse User => new(Guid.Parse("22222222-2222-2222-2222-222222222222"), "Pessoa fictícia", "pessoa@example.test", "Citizen");
    private static PriorityResult Priority => new(36, "Média", 36, 0, 0);
    private static HistoryResponse History => new(OccurrenceStatus.Open, CreatedAt);
    private static OccurrenceResponse Occurrence => new(Guid.Parse("11111111-1111-1111-1111-111111111111"), User.Id, 1, "Calçada danificada",
        "Obstáculo fictício na passagem", 3, -23.557, -46.646, CreatedAt, OccurrenceStatus.Open, Priority, false, [History], "Rua Treze de Maio, 100");

    public static JsonNode? For(Type type)
    {
        object? example = type switch
        {
            _ when type == typeof(RegisterRequest) => new RegisterRequest(User.Name, User.Email, "Senha-ficticia-123!"),
            _ when type == typeof(LoginRequest) => new LoginRequest("cidadao@passoseguro.test", "Use-DEMO_PASSWORD-do-env"),
            _ when type == typeof(RefreshRequest) => new RefreshRequest("token-ficticio-retornado-no-login"),
            _ when type == typeof(AuthResponse) => new AuthResponse("jwt-ficticio-nao-utilizavel", "refresh-ficticio-nao-utilizavel", User),
            _ when type == typeof(UserResponse) => User,
            _ when type == typeof(OccurrenceRequest) => new OccurrenceRequest(1, Occurrence.Description, 3, Occurrence.Latitude, Occurrence.Longitude, Occurrence.Address),
            _ when type == typeof(OccurrenceResponse) => Occurrence,
            _ when type == typeof(PagedResponse<OccurrenceResponse>) => new PagedResponse<OccurrenceResponse>([Occurrence], 1, 1, 30),
            _ when type == typeof(StatusRequest) => new StatusRequest(OccurrenceStatus.InReview),
            _ when type == typeof(OccurrenceStatus) => OccurrenceStatus.Open,
            _ when type == typeof(CategoryResponse) => new CategoryResponse(1, Occurrence.Category),
            _ when type == typeof(PriorityResult) => Priority,
            _ when type == typeof(HistoryResponse) => History,
            _ when type == typeof(CountResponse) => new CountResponse("Calçada danificada", 1),
            _ when type == typeof(ConcentrationResponse) => new ConcentrationResponse(Occurrence.Latitude, Occurrence.Longitude, 1),
            _ when type == typeof(IndicatorsResponse) => new IndicatorsResponse(1, [new(Occurrence.Category, 1)], [new("Open", 1)], [new("Média", 1)], [new("2026-11-13", 1)], [new(Occurrence.Latitude, Occurrence.Longitude, 1)]),
            _ when type == typeof(HealthResponse) => new HealthResponse("healthy", "PostgreSQL 16"),
            _ when type == typeof(ValidationProblemDetails) => new ValidationProblemDetails(new Dictionary<string, string[]> { ["Description"] = ["Informe uma descrição com 10 a 2000 caracteres."] }) { Status = 400, Title = "Entrada inválida." },
            _ when type == typeof(ProblemDetails) => new ProblemDetails { Status = 422, Title = "Selecione um ponto dentro de Bela Vista." },
            _ => null
        };
        return example == null ? null : JsonSerializer.SerializeToNode(example, Json);
    }

    public void Apply(IOpenApiSchema schema, SchemaFilterContext context)
    {
        // OAS 3.0 uses singular "example"; plural "examples" on schemas requires OAS 3.1.
        if (schema is OpenApiSchema concrete && For(context.Type) is { } example)
            concrete.Example = example;
    }
}
