using Barbearia.Infrastructure;
using Barbearia.Infrastructure.Dao;
using Barbearia.Infrastructure.Dao.Interface;
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

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = authenticationAuthority;
        options.Audience = authenticationAudience;
    });

builder.Services.AddAuthorization();

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

builder.Services.AddScoped<IStatusService, StatusService>();
builder.Services.AddScoped<ITipoUsuarioService, TipoUsuarioService>();
builder.Services.AddScoped<IUsuarioService, UsuarioService>();
builder.Services.AddScoped<IBarbeiroService, BarbeiroService>();
builder.Services.AddScoped<IClienteService, ClienteService>();
builder.Services.AddScoped<ICalendarioService, CalendarioService>();

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
