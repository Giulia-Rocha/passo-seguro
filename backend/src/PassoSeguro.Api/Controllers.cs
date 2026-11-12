using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using PassoSeguro.Application;
using PassoSeguro.Domain;
using PassoSeguro.Infrastructure;
namespace PassoSeguro.Api;

[ApiController, Route("api/v1/auth"), Tags("Autenticação"), EnableRateLimiting("auth")]
public sealed class AuthController(AuthService auth, AppDbContext db) : ControllerBase
{
    [HttpPost("register"), EndpointSummary("Cadastrar cidadão fictício")]
    public async Task<ActionResult<AuthResponse>> Register(RegisterRequest request,CancellationToken ct)=>Ok(await auth.Register(request,ct));
    [HttpPost("login"), EndpointSummary("Entrar e obter tokens")]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest request,CancellationToken ct) { var result=await auth.Login(request,ct); return result==null ? Unauthorized(new ProblemDetails { Status=401,Title="E-mail ou senha inválidos." }) : Ok(result); }
    [HttpPost("refresh"), EndpointSummary("Renovar sessão e invalidar token anterior")]
    public async Task<ActionResult<AuthResponse>> Refresh(RefreshRequest request,CancellationToken ct) { var result=await auth.Refresh(request.RefreshToken,ct); return result==null ? Unauthorized() : Ok(result); }
    [HttpPost("logout"), EndpointSummary("Revogar token de renovação")]
    public async Task<IActionResult> Logout(RefreshRequest request,CancellationToken ct) { await auth.Logout(request.RefreshToken,ct); return NoContent(); }
    [Authorize, HttpGet("me"), EndpointSummary("Consultar perfil atual")]
    public async Task<ActionResult<UserResponse>> Me(CancellationToken ct)
    {
        var id=Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var user=await db.Citizens.FindAsync([id],ct); return user==null ? Unauthorized() : Ok(new UserResponse(user.Id,user.Name,user.Email,user.Role));
    }
}
[ApiController, Authorize, Route("api/v1/occurrences"), Tags("Ocorrências")]
public sealed class OccurrencesController(OccurrenceService service,AppDbContext db) : ControllerBase
{
    private Guid Actor=>Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    [HttpGet, EndpointSummary("Listar ocorrências com filtros e paginação")]
    public async Task<ActionResult<PagedResponse<OccurrenceResponse>>> List([FromQuery] OccurrenceFilter filter,CancellationToken ct)=>Ok(await service.List(filter,Actor,ct));
    [HttpGet("{id:guid}"), EndpointSummary("Consultar detalhe, prioridade e histórico")]
    public async Task<ActionResult<OccurrenceResponse>> Get(Guid id,CancellationToken ct)=>Ok(await service.Detail(id,ct));
    [HttpPost, EndpointSummary("Registrar ocorrência com chave idempotente")]
    public async Task<ActionResult<OccurrenceResponse>> Create(OccurrenceRequest request,[FromHeader(Name="Idempotency-Key")] Guid key,CancellationToken ct)
    { var result=await service.Create(Actor,key,request,ct); return CreatedAtAction(nameof(Get),new { id=result.Id },result); }
    [HttpPut("{id:guid}"), EndpointSummary("Editar ocorrência aberta do autor")]
    public async Task<ActionResult<OccurrenceResponse>> Edit(Guid id,OccurrenceRequest request,CancellationToken ct)=>Ok(await service.Edit(id,Actor,request,ct));
    [HttpDelete("{id:guid}"), EndpointSummary("Excluir ocorrência aberta do autor")]
    public async Task<IActionResult> Delete(Guid id,CancellationToken ct) { await service.Delete(id,Actor,ct); return NoContent(); }
    [HttpPatch("{id:guid}/status"), Authorize(Roles="Manager"), EndpointSummary("Atualizar andamento como gestor")]
    public async Task<ActionResult<OccurrenceResponse>> Status(Guid id,StatusRequest request,CancellationToken ct)=>Ok(await service.ChangeStatus(id,Actor,true,request.Status,ct));
    [HttpGet("{id:guid}/history"), EndpointSummary("Consultar histórico de status")]
    public async Task<ActionResult<IReadOnlyList<HistoryResponse>>> History(Guid id,CancellationToken ct)=>Ok((await service.Detail(id,ct)).History);
    [HttpPut("{id:guid}/photo"), Consumes("multipart/form-data"), RequestSizeLimit(6*1024*1024), EndpointSummary("Enviar foto JPEG ou PNG até 5 MB")]
    public async Task<IActionResult> Photo(Guid id,IFormFile file,CancellationToken ct)
    {
        var value=await service.Required(id,ct); if(value.AuthorId!=Actor) throw new ForbiddenException(); value.EnsureEditable(Actor);
        if(file.Length>5*1024*1024) return Problem(statusCode:413,title:"A foto deve ter no máximo 5 MB.");
        using var stream=new MemoryStream(); await file.CopyToAsync(stream,ct); var bytes=stream.ToArray();
        var png=bytes.Length>=8 && bytes.AsSpan(0,8).SequenceEqual(new byte[]{137,80,78,71,13,10,26,10});
        var jpeg=bytes.Length>=4 && bytes[0]==255 && bytes[1]==216 && bytes[2]==255 && bytes[^2]==255 && bytes[^1]==217;
        if(!png&&!jpeg) return Problem(statusCode:415,title:"Envie uma imagem JPEG ou PNG válida.");
        var photo=await db.Photos.FindAsync([id],ct);
        if(photo==null) { photo=new OccurrencePhoto { OccurrenceId=id }; db.Photos.Add(photo); }
        photo.Content=bytes; photo.ContentType=png?"image/png":"image/jpeg"; await db.SaveChangesAsync(ct); return NoContent();
    }
    [HttpGet("{id:guid}/photo"), EndpointSummary("Consultar fotografia da ocorrência")]
    public async Task<IActionResult> GetPhoto(Guid id,CancellationToken ct)
    { var photo=await db.Photos.AsNoTracking().FirstOrDefaultAsync(x=>x.OccurrenceId==id,ct) ?? throw new MissingException(); return File(photo.Content,photo.ContentType); }
    [HttpDelete("{id:guid}/photo"), EndpointSummary("Remover foto da ocorrência aberta")]
    public async Task<IActionResult> DeletePhoto(Guid id,CancellationToken ct)
    { var value=await service.Required(id,ct); if(value.AuthorId!=Actor) throw new ForbiddenException(); value.EnsureEditable(Actor); var photo=await db.Photos.FindAsync([id],ct); if(photo!=null) db.Photos.Remove(photo); await db.SaveChangesAsync(ct); return NoContent(); }
}
[ApiController, Authorize, Route("api/v1"), Tags("Consultas")]
public sealed class QueriesController(IOccurrenceStore store,ITerritory territory,OccurrenceService service,AppDbContext db) : ControllerBase
{
    [HttpGet("categories"), EndpointSummary("Listar categorias ativas")]
    public async Task<ActionResult<List<CategoryResponse>>> Categories(CancellationToken ct)=>Ok((await store.Categories(ct)).Select(x=>new CategoryResponse(x.Id,x.Name)));
    [HttpGet("territory"), EndpointSummary("Consultar polígono público de Bela Vista em GeoJSON")]
    public IActionResult Territory()=>Ok(territory.GeoJson);
    [HttpGet("indicators"), EndpointSummary("Consultar indicadores e concentrações para os filtros")]
    public async Task<ActionResult<IndicatorsResponse>> Indicators([FromQuery] OccurrenceFilter filter,CancellationToken ct)=>Ok(await service.Indicators(filter,Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!),ct));
    [AllowAnonymous,HttpGet("health"), EndpointSummary("Verificar API e conexão com PostgreSQL")]
    public async Task<IActionResult> Health(CancellationToken ct)=>await db.Database.CanConnectAsync(ct) ? Ok(new { status="healthy",database="PostgreSQL 16" }) : StatusCode(503,new { status="unavailable" });
}
