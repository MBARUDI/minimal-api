using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Backend.Dominio.Entidades; // Corrigido
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authorization;
using Backend.Infraestrutura.Db; // Corrigido
using Backend.Dominio.ModelViews; // Corrigido
using Backend.DTOs; // Corrigido
using Backend.Dominio.Enums; // Corrigido
using System.Text;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using BCrypt.Net; // Mantido para acesso à classe BCrypt

var builder = WebApplication.CreateBuilder(args);

// 🔧 Configuração do DbContext com MySQL
var connectionString = builder.Configuration.GetConnectionString("MySql");
builder.Services.AddDbContext<DbContexto>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

//  Configuração do CORS
var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";
builder.Services.AddCors(options => {
    options.AddPolicy(name: MyAllowSpecificOrigins, policy  => { policy.WithOrigins("http://localhost:4200").AllowAnyHeader().AllowAnyMethod(); }); // Angular default port
});

// 🔐 Configuração do JWT
var jwtKey = builder.Configuration["Jwt:Key"] ?? string.Empty;
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? string.Empty;

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

builder.Services.AddAuthorization(options =>
{
    // Política para acesso de Administrador: requer o perfil 'Adm'
    options.AddPolicy("AcessoDeAdministrador", policy =>
        policy.RequireRole(Perfil.Adm.ToString()));

    // Política para acesso de Editor: requer o perfil 'Editor' OU 'Adm'
    options.AddPolicy("AcessoDeEditor", policy =>
        policy.RequireRole(Perfil.Adm.ToString(), Perfil.Editor.ToString()));
});

// 📦 Swagger com suporte a JWT
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Minimal API",
        Version = "v1",
        Description = "API minimalista com autenticação JWT e MySQL"
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Insira o token JWT no formato: Bearer {seu_token}"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

var app = builder.Build();

// 🚀 Pipeline HTTP
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Minimal API v1");
    c.RoutePrefix = string.Empty;
});

app.UseCors(MyAllowSpecificOrigins);

app.UseAuthentication();
app.UseAuthorization();

// 🏠 Endpoint Home
app.MapGet("/", () => Results.Ok(new Home()))
    .WithTags("Home");

// 🔐 Endpoint de login com retorno de AdministradorLogado
app.MapPost("/login", async (LoginDTO login, DbContexto db, IConfiguration config) =>
{
    var usuario = await db.Administradores
        .FirstOrDefaultAsync(a => a.Email == login.Email);

    if (usuario is null || !BCrypt.Net.BCrypt.Verify(login.Senha, usuario.Senha))
    {
        return Results.Unauthorized();
    }

    var claims = new[]
    {
        new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
        new Claim(ClaimTypes.Email, usuario.Email),
        new Claim(ClaimTypes.Role, usuario.Perfil.ToString())
    };

    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"] ?? ""));
    var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

    var token = new JwtSecurityToken(
        issuer: config["Jwt:Issuer"] ?? "",
        audience: null,
        claims: claims,
        expires: DateTime.Now.AddHours(2),
        signingCredentials: creds
    );

    var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

    // ✅ Conversão segura de string para enum Perfil
    var perfilConvertido = Enum.TryParse<Perfil>(usuario.Perfil.ToString(), out var perfilValido)
        ? perfilValido
        : Perfil.Adm;

    var administradorLogado = new AdministradorLogado
    {
        Email = usuario.Email,
        Perfil = perfilConvertido,
        Token = tokenString
    };

    return Results.Ok(administradorLogado);
});
    

// 🚗 Endpoints CRUD para Veículos

// GET /veiculos - Listar todos os veículos
app.MapGet("/veiculos", async (DbContexto db) =>
{
    return await db.Veiculos.ToListAsync();
})
.WithTags("Veiculos");

// GET /veiculos/{id} - Obter veículo por ID
app.MapGet("/veiculos/{id:int}", async (int id, DbContexto db) =>
{
    var veiculo = await db.Veiculos.FindAsync(id);
    return veiculo is not null ? Results.Ok(veiculo) : Results.NotFound();
})
.WithTags("Veiculos");

// POST /veiculos - Criar um novo veículo
app.MapPost("/veiculos", [Authorize(Policy = "AcessoDeEditor")] async (VeiculoDTO veiculoDto, DbContexto db) =>
{
    // Validação manual
    if (string.IsNullOrWhiteSpace(veiculoDto.Nome) || string.IsNullOrWhiteSpace(veiculoDto.Marca))
    {
        return Results.BadRequest("Nome e Marca são obrigatórios.");
    }
    if (veiculoDto.Ano <= 0)
    {
        return Results.BadRequest("Ano deve ser um valor positivo.");
    }

    var veiculo = new Veiculo
    {
        Nome = veiculoDto.Nome,
        Marca = veiculoDto.Marca,
        Ano = veiculoDto.Ano
    };

    db.Veiculos.Add(veiculo);
    await db.SaveChangesAsync();
    return Results.Created($"/veiculos/{veiculo.Id}", veiculo);
})
.WithTags("Veiculos");

// PUT /veiculos/{id} - Atualizar um veículo existente
app.MapPut("/veiculos/{id:int}", [Authorize(Policy = "AcessoDeEditor")] async (int id, VeiculoDTO veiculoDto, DbContexto db) =>
{
    // Validação manual
    if (string.IsNullOrWhiteSpace(veiculoDto.Nome) || string.IsNullOrWhiteSpace(veiculoDto.Marca))
    {
        return Results.BadRequest("Nome e Marca são obrigatórios.");
    }
    if (veiculoDto.Ano <= 0)
    {
        return Results.BadRequest("Ano deve ser um valor positivo.");
    }

    var veiculoExistente = await db.Veiculos.FindAsync(id);
    if (veiculoExistente is null)
    {
        return Results.NotFound();
    }

    veiculoExistente.Nome = veiculoDto.Nome;
    veiculoExistente.Marca = veiculoDto.Marca;
    veiculoExistente.Ano = veiculoDto.Ano;

    await db.SaveChangesAsync();
    return Results.NoContent();
})
.WithTags("Veiculos");

// DELETE /veiculos/{id} - Deletar um veículo
app.MapDelete("/veiculos/{id:int}", [Authorize(Policy = "AcessoDeEditor")] async (int id, DbContexto db) =>
{
    var veiculo = await db.Veiculos.FindAsync(id);
    if (veiculo is null) return Results.NotFound();

    db.Veiculos.Remove(veiculo);
    await db.SaveChangesAsync();
    return Results.NoContent();
})
.WithTags("Veiculos");

// 👨‍💼 Endpoints CRUD para Administradores

// GET /administradores - Listar todos os administradores
app.MapGet("/administradores", [Authorize(Policy = "AcessoDeAdministrador")] async (DbContexto db) =>
{
    var administradores = await db.Administradores
        .Select(a => new AdministradorModelView { Id = a.Id, Email = a.Email, Perfil = Enum.Parse<Perfil>(a.Perfil) })
        .ToListAsync();
    return Results.Ok(administradores);
})
.WithTags("Administradores");

// GET /administradores/{id} - Obter administrador por ID
app.MapGet("/administradores/{id:int}", [Authorize(Policy = "AcessoDeAdministrador")] async (int id, DbContexto db) =>
{
    var administrador = await db.Administradores.FindAsync(id);
    if (administrador is null) return Results.NotFound();

    var viewModel = new AdministradorModelView
    {
        Id = administrador.Id,
        Email = administrador.Email,
        Perfil = Enum.Parse<Perfil>(administrador.Perfil)
    };
    return Results.Ok(viewModel);
})
.WithTags("Administradores");

// POST /administradores - Criar um novo administrador
app.MapPost("/administradores", [Authorize(Policy = "AcessoDeAdministrador")] async (Administrador administrador, DbContexto db) =>
{
    // Validação básica
    if (string.IsNullOrWhiteSpace(administrador.Email) || string.IsNullOrWhiteSpace(administrador.Senha))
    {
        return Results.BadRequest("Email e Senha são obrigatórios.");
    }

    // Hash da senha antes de salvar
    administrador.Senha = BCrypt.Net.BCrypt.HashPassword(administrador.Senha);

    db.Administradores.Add(administrador);
    await db.SaveChangesAsync();

    var viewModel = new AdministradorModelView { Id = administrador.Id, Email = administrador.Email, Perfil = Enum.Parse<Perfil>(administrador.Perfil) };
    return Results.Created($"/administradores/{administrador.Id}", viewModel);
})
.WithTags("Administradores");

// DELETE /administradores/{id} - Deletar um administrador
app.MapDelete("/administradores/{id:int}", [Authorize(Policy = "AcessoDeAdministrador")] async (int id, DbContexto db) =>
{
    var administrador = await db.Administradores.FindAsync(id);
    if (administrador is null) return Results.NotFound();

    db.Administradores.Remove(administrador);
    await db.SaveChangesAsync();
    return Results.NoContent();
})
.WithTags("Administradores");

app.Run();
