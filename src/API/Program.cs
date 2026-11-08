using Barbearia.Api.Authorization;
using Barbearia.Application.Abstractions;
using Barbearia.Infrastructure;
using Barbearia.Infrastructure.Dao;
using Barbearia.Infrastructure.Dao.Interface;
using Barbearia.Infrastructure.Messaging;
using Barbearia.Infrastructure.Security;
using Barbearia.Infrastructure.Service;
using Barbearia.Infrastructure.Service.Interface;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

const string FrontendCorsPolicy = "FrontendCorsPolicy";

builder.Services.AddCors(options =>
{
    options.AddPolicy(FrontendCorsPolicy, policy =>
        policy.WithOrigins("http://localhost:5174", "http://localhost:8082")
            .AllowAnyHeader()
            .AllowAnyMethod());
});

var authenticationAuthority = builder.Configuration["Authentication:Authority"]
    ?? throw new InvalidOperationException("Authentication:Authority não configurado.");
var authenticationAudience = builder.Configuration["Authentication:Audience"]
    ?? throw new InvalidOperationException("Authentication:Audience não configurado.");
// O "iss" emitido nos tokens é a URL pela qual o navegador acessa o Authentication
// (ex.: https://localhost:8443/). Authority pode ser outra URL (ex.: via Docker,
// host.docker.internal) só para esta API buscar o documento de metadados/JWKS, por
// isso o issuer esperado na validação precisa ser configurável separadamente.
var authenticationIssuer = builder.Configuration["Authentication:Issuer"] ?? authenticationAuthority;

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = authenticationAuthority;
        options.Audience = authenticationAudience;
        options.TokenValidationParameters.ValidIssuer = authenticationIssuer;

        if (builder.Environment.IsDevelopment())
        {
            // O certificado HTTPS de desenvolvimento do Authentication é autoassinado
            // (só o navegador do host confia nele, manualmente). Isso permite que esta
            // API, rodando em outro container, busque o documento de metadados OIDC e
            // o JWKS sem precisar importar esse certificado no container.
            options.BackchannelHttpHandler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator,
            };
        }
    });

builder.Services.AddAuthorization(options =>
{
    // RequireAssertion em vez de Roles= no [Authorize]: o handler JWT pode manter
    // o claim de papel como "role" (nome curto emitido pelo OpenIddict) ou remapear
    // para a URI longa ClaimTypes.Role, dependendo do ambiente (mesma ambiguidade
    // já tratada em ObterAuthUserId() para o claim "sub") — checar os dois evita
    // depender de qual delas está em vigor.
    options.AddPolicy(Roles.Administrador, policy =>
        policy.RequireAssertion(context =>
            context.User.Claims.Any(claim =>
                (claim.Type == "role" || claim.Type == System.Security.Claims.ClaimTypes.Role)
                && claim.Value == Roles.Administrador)));
});

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.OpenApiInfo
    {
        Title = "Barbearia API",
        Version = "v1"
    });
});

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddMediatR(config =>
    config.RegisterServicesFromAssembly(typeof(Barbearia.Application.AssemblyReference).Assembly));

builder.Services.AddValidatorsFromAssembly(typeof(Barbearia.Application.AssemblyReference).Assembly);

builder.Services.AddScoped<IStatusDao, StatusDao>();
builder.Services.AddScoped<ITipoUsuarioDao, TipoUsuarioDao>();
builder.Services.AddScoped<IUsuarioDao, UsuarioDao>();
builder.Services.AddScoped<IBarbeiroDao, BarbeiroDao>();
builder.Services.AddScoped<IClienteDao, ClienteDao>();
builder.Services.AddScoped<ICalendarioDao, CalendarioDao>();
builder.Services.AddScoped<ILogAlteracaoDao, LogAlteracaoDao>();

builder.Services.AddScoped<IStatusService, StatusService>();
builder.Services.AddScoped<ITipoUsuarioService, TipoUsuarioService>();
builder.Services.AddScoped<IUsuarioService, UsuarioService>();
builder.Services.AddScoped<IBarbeiroService, BarbeiroService>();
builder.Services.AddScoped<IClienteService, ClienteService>();
builder.Services.AddScoped<ICalendarioService, CalendarioService>();
builder.Services.AddScoped<ILogAlteracaoService, LogAlteracaoService>();

// RabbitMQ / Log de Alterações
builder.Services.Configure<RabbitMqOptions>(
    builder.Configuration.GetSection(RabbitMqOptions.SectionName));
builder.Services.AddHttpContextAccessor();
builder.Services.AddSingleton<IRabbitMqConnectionProvider, RabbitMqConnectionProvider>();
builder.Services.AddScoped<ILogAlteracaoPublisher, RabbitMqLogAlteracaoPublisher>();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddHostedService<LogAlteracaoConsumer>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Barbearia API v1");
});

app.UseHttpsRedirection();

app.UseCors(FrontendCorsPolicy);

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
