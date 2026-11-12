using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Authorization;
using PassoSeguro.Application;
namespace PassoSeguro.Api;

public sealed class ApiDocumentation : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (context.MethodInfo.GetCustomAttributes(typeof(AllowAnonymousAttribute), true).Length == 0 && (context.MethodInfo.DeclaringType!.GetCustomAttributes(typeof(AuthorizeAttribute), true).Length > 0 || context.MethodInfo.GetCustomAttributes(typeof(AuthorizeAttribute), true).Length > 0))
            operation.Security = [new OpenApiSecurityRequirement { [new OpenApiSecuritySchemeReference("Bearer", context.Document)] = [] }];
        foreach (var (code, message) in new[] { ("400", "Entrada inválida"), ("401", "Sessão ausente ou expirada"), ("403", "Operação sem permissão"), ("404", "Registro inexistente"), ("409", "Conflito de atualização ou chave"), ("422", "Regra de negócio violada"), ("429", "Limite de tentativas") })
            operation.Responses!.TryAdd(code, new OpenApiResponse { Description = message });
        if (context.MethodInfo.Name == "Photo") { operation.Responses!.TryAdd("413", new OpenApiResponse { Description = "Foto acima de 5 MB" }); operation.Responses.TryAdd("415", new OpenApiResponse { Description = "Formato inválido" }); }
    }
}
public sealed class ApiExamples : ISchemaFilter
{
    public void Apply(IOpenApiSchema schema, SchemaFilterContext context)
    {
        var example = context.Type.Name switch
        {
            nameof(RegisterRequest) => """{"name":"Pessoa fictícia","email":"pessoa@example.test","password":"Use-a-senha-local-123!"}""",
            nameof(LoginRequest) => """{"email":"cidadao@passoseguro.test","password":"Use-DEMO_PASSWORD-do-env"}""",
            nameof(RefreshRequest) => """{"refreshToken":"token-retornado-no-login"}""",
            nameof(OccurrenceRequest) => """{"categoryId":1,"description":"Obstáculo fictício na passagem de pedestres","difficulty":3,"latitude":-23.557,"longitude":-46.646}""",
            nameof(StatusRequest) => """{"status":"InReview"}""",
            nameof(CategoryResponse) => """{"id":1,"name":"Calçada danificada"}""",
            nameof(UserResponse) => """{"id":"11111111-1111-1111-1111-111111111111","name":"Pessoa fictícia","email":"pessoa@example.test","role":"Citizen"}""",
            nameof(OccurrenceResponse) => """{"id":"11111111-1111-1111-1111-111111111111","authorId":"22222222-2222-2222-2222-222222222222","categoryId":1,"category":"Calçada danificada","description":"Obstáculo fictício na passagem","difficulty":3,"latitude":-23.557,"longitude":-46.646,"createdAt":"2026-11-11T12:00:00Z","status":"Open","priority":{"score":36,"level":"Média","severityPoints":36,"recurrencePoints":0,"agePoints":0},"hasPhoto":false,"history":[{"status":"Open","createdAt":"2026-11-11T12:00:00Z"}]}""",
            _ => null
        };
        if (example != null && schema is OpenApiSchema concrete) concrete.Example = JsonNode.Parse(example);
    }
}
