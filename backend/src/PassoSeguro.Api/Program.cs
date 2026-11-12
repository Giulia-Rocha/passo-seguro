using System.Text;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Npgsql;
using PassoSeguro.Api;
using PassoSeguro.Application;
using PassoSeguro.Domain;
using PassoSeguro.Infrastructure;
var builder=WebApplication.CreateBuilder(args);
var key=builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("Configure Jwt__Key.");
if(Encoding.UTF8.GetByteCount(key)<32) throw new InvalidOperationException("Jwt__Key exige pelo menos 32 bytes.");
builder.Services.AddDbContext<AppDbContext>(o=>o.UseNpgsql(builder.Configuration.GetConnectionString("Database") ?? throw new InvalidOperationException("Configure ConnectionStrings__Database.")));
builder.Services.AddScoped<IOccurrenceStore>(sp=>sp.GetRequiredService<AppDbContext>());
builder.Services.AddSingleton<ITerritory,Territory>(); builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddScoped<OccurrenceService>(); builder.Services.AddScoped<AuthService>();
builder.Services.AddControllers().AddJsonOptions(o=>o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(o=> { o.SwaggerDoc("v1",new() { Title="Passo Seguro",Version="v1",Description="Protótipo acadêmico de acessibilidade. Dados demonstrativos e prioridade sem caráter oficial." }); o.OperationFilter<ApiDocumentation>(); });
builder.Services.AddProblemDetails();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(o=>o.TokenValidationParameters=new() {
    ValidateIssuer=true,ValidIssuer="PassoSeguro",ValidateAudience=true,ValidAudience="PassoSeguro.Mobile",
    ValidateIssuerSigningKey=true,IssuerSigningKey=new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),ValidateLifetime=true,ClockSkew=TimeSpan.FromSeconds(15)
});
builder.Services.AddAuthorization();
builder.Services.AddRateLimiter(o=> { o.RejectionStatusCode=429; o.AddPolicy("auth",http=>RateLimitPartition.GetFixedWindowLimiter(http.Connection.RemoteIpAddress?.ToString() ?? "local",_=>new() { PermitLimit=30,Window=TimeSpan.FromMinutes(1),QueueLimit=0 })); });
var app=builder.Build();
app.UseExceptionHandler(error=>error.Run(async context=> {
    var exception=context.Features.Get<IExceptionHandlerFeature>()!.Error;
    var status=exception switch { MissingException=>404,ForbiddenException=>403,RuleException=>422,ConflictException=>409,
        DbUpdateConcurrencyException=>409,DbUpdateException { InnerException:PostgresException { SqlState:"23505" } }=>409,_=>500 };
    await Results.Problem(statusCode:status,title:status==500 ? "Não foi possível concluir a operação." : status==409 ? "Conflito de atualização" : exception.Message,
        detail:status==409 ? "Atualize a consulta antes de tentar novamente. Em caso de envio repetido, mantenha a mesma chave." : null).ExecuteAsync(context);
}));
app.UseSwagger(); app.UseSwaggerUI(); app.UseRateLimiter(); app.UseAuthentication(); app.UseAuthorization(); app.MapControllers();
if(args.Contains("--migrate"))
{
    using var scope=app.Services.CreateScope(); var db=scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
    await DemoSeed.Apply(db,builder.Configuration["Demo:Password"] ?? throw new InvalidOperationException("Configure Demo__Password."));
    return;
}
app.Run();
public partial class Program;
