using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PassoSeguro.Application;
using PassoSeguro.Domain;
using PassoSeguro.Infrastructure;
namespace PassoSeguro.Api;
public sealed class AuthService(AppDbContext db, IConfiguration config, TimeProvider clock)
{
    private readonly PasswordHasher<Citizen> hasher = new();
    public async Task<AuthResponse> Register(RegisterRequest request, CancellationToken ct)
    {
        var email=request.Email.Trim().ToLowerInvariant();
        if(await db.Citizens.AnyAsync(x=>x.Email==email,ct)) throw new ConflictException("Este e-mail já está cadastrado.");
        if(string.IsNullOrWhiteSpace(request.Name) || request.Name.Trim().Length<2) throw new RuleException("Informe um nome com ao menos 2 caracteres.");
        var value=new Citizen { Email=email,Name=request.Name.Trim() };
        value.PasswordHash=hasher.HashPassword(value,request.Password);
        db.Citizens.Add(value); return await Issue(value,ct);
    }
    public async Task<AuthResponse?> Login(LoginRequest request, CancellationToken ct)
    {
        var value=await db.Citizens.FirstOrDefaultAsync(x=>x.Email==request.Email.Trim().ToLowerInvariant(),ct);
        var candidate=value ?? new Citizen { PasswordHash=hasher.HashPassword(new Citizen(),"invalid-placeholder") };
        if(hasher.VerifyHashedPassword(candidate,candidate.PasswordHash,request.Password)==PasswordVerificationResult.Failed || value==null) return null;
        return await Issue(value,ct);
    }
    public async Task<AuthResponse?> Refresh(string token, CancellationToken ct)
    {
        var hash=Hash(token);
        var session=await db.Sessions.Include(x=>x.Citizen).FirstOrDefaultAsync(x=>x.TokenHash==hash,ct);
        if(session==null || session.Revoked || session.ExpiresAt<=clock.GetUtcNow()) return null;
        session.Revoked=true; return await Issue(session.Citizen,ct);
    }
    public async Task Logout(string token, CancellationToken ct)
    {
        var hash=Hash(token); var session=await db.Sessions.FirstOrDefaultAsync(x=>x.TokenHash==hash,ct);
        if(session!=null) { session.Revoked=true; await db.SaveChangesAsync(ct); }
    }
    private async Task<AuthResponse> Issue(Citizen value, CancellationToken ct)
    {
        var refresh=Convert.ToBase64String(RandomNumberGenerator.GetBytes(48));
        db.Sessions.Add(new RefreshSession { CitizenId=value.Id,TokenHash=Hash(refresh),ExpiresAt=clock.GetUtcNow().AddDays(7) });
        await db.SaveChangesAsync(ct);
        var claims=new[] { new Claim(ClaimTypes.NameIdentifier,value.Id.ToString()),new Claim(ClaimTypes.Role,value.Role) };
        var token=new JwtSecurityToken("PassoSeguro","PassoSeguro.Mobile",claims,expires:clock.GetUtcNow().AddMinutes(15).UtcDateTime,
            signingCredentials:new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"]!)),SecurityAlgorithms.HmacSha256));
        return new(new JwtSecurityTokenHandler().WriteToken(token),refresh,new(value.Id,value.Name,value.Email,value.Role));
    }
    private static string Hash(string value)=>Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value)));
}
