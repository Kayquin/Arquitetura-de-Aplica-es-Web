using System.Reflection;
using System.Text;
using AgendaConsultas.Api.Data;
using AgendaConsultas.Api.Repositories;
using AgendaConsultas.Api.Services;
using AgendaConsultas.Api.Settings;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

// Bootstrap da API e configuracao do pipeline HTTP.
var builder = WebApplication.CreateBuilder(args);

// Habilita controllers e validacao automatica de modelos.
builder.Services.AddControllers();

// Carrega configuracoes de MongoDB e JWT.
builder.Services.Configure<MongoDbSettings>(builder.Configuration.GetSection("Mongo"));
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));
builder.Services.AddSingleton<MongoDbContext>();

// Registra repositorios e servicos no DI.
builder.Services.AddScoped<IPacienteRepository, PacienteRepository>();
builder.Services.AddScoped<IConsultaRepository, ConsultaRepository>();
builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();

builder.Services.AddScoped<IPacienteService, PacienteService>();
builder.Services.AddScoped<IConsultaService, ConsultaService>();
builder.Services.AddScoped<IUsuarioService, UsuarioService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IAuthService, AuthService>();

// Permite chamadas do frontend local.
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

// Validacao JWT para endpoints protegidos.
var jwtSettings = builder.Configuration.GetSection("Jwt").Get<JwtSettings>() ?? new JwtSettings();
var jwtKey = Encoding.UTF8.GetBytes(jwtSettings.Key ?? string.Empty);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(jwtKey),
            ClockSkew = TimeSpan.FromMinutes(1)
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new Microsoft.AspNetCore.Mvc.ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
});

builder.Services.AddVersionedApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});

builder.Services.AddEndpointsApiExplorer();
// Swagger v1/v2: informacoes da API, seguranca JWT e comentarios XML.
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Agenda Consultas API",
        Version = "v1",
        Description = "Versao anterior da API REST para gerenciamento de consultas medicas.",
        Contact = new OpenApiContact
        {
            Name = "Suporte",
            Email = "suporte@agendaconsultas.com"
        }
    });

    options.SwaggerDoc("v2", new OpenApiInfo
    {
        Title = "Agenda Consultas API",
        Version = "v2",
        Description = "API REST para gerenciamento de consultas medicas. " +
                      "Suporta autenticacao JWT, CRUD completo de pacientes e consultas, " +
                      "atualizacao parcial via PATCH e controle de acesso por roles (admin/usuario).",
        Contact = new OpenApiContact
        {
            Name = "Suporte",
            Email = "suporte@agendaconsultas.com"
        }
    });

    options.DocInclusionPredicate((docName, apiDesc) =>
    {
        var groupName = apiDesc.GroupName;
        if (string.IsNullOrWhiteSpace(groupName))
        {
            return docName == "v1";
        }

        return string.Equals(groupName, docName, StringComparison.OrdinalIgnoreCase);
    });

    // Define o esquema Bearer JWT para autenticacao.
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Informe o token JWT no formato: Bearer {seu_token}"
    });

    // Aplica o requisito de seguranca globalmente em todos os endpoints.
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
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
            Array.Empty<string>()
        }
    });

    // Inclui comentarios XML dos controllers e DTOs.
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    options.IncludeXmlComments(xmlPath);
});

var app = builder.Build();

app.UseCors("AllowAll");

if (app.Environment.IsDevelopment())
{
    // Swagger v2 com UI personalizada e rota do endpoint.
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Agenda Consultas API v1");
        options.SwaggerEndpoint("/swagger/v2/swagger.json", "Agenda Consultas API v2");
        options.RoutePrefix = "swagger";
        options.DocumentTitle = "Agenda Consultas — Swagger UI";
    });
}

// Serve o frontend de /frontend quando existir.
var frontendPath = Path.GetFullPath(Path.Combine(app.Environment.ContentRootPath, "..", "..", "frontend"));
if (Directory.Exists(frontendPath))
{
    // Usa index.html como default e expõe arquivos estaticos.
    app.UseDefaultFiles(new DefaultFilesOptions
    {
        FileProvider = new PhysicalFileProvider(frontendPath)
    });
    app.UseStaticFiles(new StaticFileOptions
    {
        FileProvider = new PhysicalFileProvider(frontendPath)
    });
}

app.UseHttpsRedirection();

// Pipeline de auth: JWT depois autorizacao.
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
