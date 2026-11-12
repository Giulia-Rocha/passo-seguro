using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;
namespace PassoSeguro.Api;
public sealed class ApiDocumentation : IOperationFilter
{
    public void Apply(OpenApiOperation operation,OperationFilterContext context)
    {
        foreach(var (code,message) in new[] { ("400","Entrada inválida"),("401","Sessão ausente ou expirada"),("403","Operação sem permissão"),("404","Registro inexistente"),("409","Conflito de atualização ou chave"),("422","Regra de negócio violada"),("429","Limite de tentativas") })
            operation.Responses!.TryAdd(code,new OpenApiResponse { Description=message });
        if(context.MethodInfo.Name=="Photo") { operation.Responses!.TryAdd("413",new OpenApiResponse { Description="Foto acima de 5 MB" }); operation.Responses.TryAdd("415",new OpenApiResponse { Description="Formato inválido" }); }
    }
}
