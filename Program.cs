using CreditoAPI.BackgroundServices;
using CreditoAPI.Data;
using CreditoAPI.Infrastructure.Authentication;
using CreditoAPI.Infrastructure.Cache;
using CreditoAPI.Infrastructure.Resilience;
using CreditoAPI.Infrastructure.UnitOfWork;
using CreditoAPI.Infrastructure.EventSourcing;
using CreditoAPI.Infrastructure.Saga;
using CreditoAPI.Repositories;
using CreditoAPI.Services;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Prometheus;
using StackExchange.Redis;
using System.Reflection;
using System.Text;
using AspNetCoreRateLimit;
using Asp.Versioning;
using Marten;
using MassTransit;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// API Versioning
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new Asp.Versioning.ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
}).AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Credito API - CQRS Pattern",
        Version = "v1.0",
        Description = "API para consulta de créditos constituídos usando CQRS, MediatR e Design Patterns"
    });
    
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    
    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// JWT Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
builder.Services.Configure<JwtSettings>(jwtSettings);

var key = Encoding.UTF8.GetBytes(jwtSettings["Secret"]!);
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidateAudience = true,
        ValidAudience = jwtSettings["Audience"],
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();

// Rate Limiting
builder.Services.AddMemoryCache();
builder.Services.Configure<IpRateLimitOptions>(builder.Configuration.GetSection("IpRateLimiting"));
builder.Services.AddInMemoryRateLimiting();
builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();

// Redis Cache
var redisConnection = builder.Configuration.GetConnectionString("Redis");
if (!string.IsNullOrEmpty(redisConnection))
{
    builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
    {
        try
        {
            return ConnectionMultiplexer.Connect(redisConnection);
        }
        catch (Exception ex)
        {
            var logger = sp.GetRequiredService<ILogger<Program>>();
            logger.LogWarning(ex, "Failed to connect to Redis. Cache will not be available.");
            return null!;
        }
    });
    builder.Services.AddSingleton<ICacheService, RedisCacheService>();
}

// MediatR - CQRS Pattern
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

// FluentValidation
builder.Services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

// Database configuration
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Health checks
builder.Services.AddHealthChecks()
    .AddDbContextCheck<ApplicationDbContext>("database");

// Repository pattern
builder.Services.AddScoped<ICreditoRepository, CreditoRepository>();

// Unit of Work pattern
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Services with Circuit Breaker and Retry Policy
builder.Services.AddScoped<ICreditoService, CreditoService>();
builder.Services.AddSingleton<ServiceBusService>();
builder.Services.AddSingleton<IServiceBusService>(sp =>
{
    var innerService = sp.GetRequiredService<ServiceBusService>();
    var logger = sp.GetRequiredService<ILogger<ResilientServiceBusService>>();
    return new ResilientServiceBusService(innerService, logger);
});

// Event Sourcing with Marten
var eventStoreConnection = builder.Configuration.GetConnectionString("EventStore") 
    ?? builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddMarten(options =>
{
    options.Connection(eventStoreConnection!);
    options.AutoCreateSchemaObjects = Weasel.Core.AutoCreate.All;
});

builder.Services.AddScoped<IEventStore, MartenEventStore>();

// Saga Pattern with MassTransit
builder.Services.AddMassTransit(x =>
{
    x.AddSagaStateMachine<CreditoSagaStateMachine, CreditoSagaState>()
        .InMemoryRepository();

    x.UsingAzureServiceBus((context, cfg) =>
    {
        var serviceBusConnection = builder.Configuration.GetSection("ServiceBus:ConnectionString").Value;
        cfg.Host(serviceBusConnection);
        cfg.ConfigureEndpoints(context);
    });
});

// API Gateway with Ocelot
builder.Configuration.AddJsonFile("ocelot.json", optional: true, reloadOnChange: true);
builder.Services.AddOcelot();

// Background service
builder.Services.AddHostedService<CreditoProcessorService>();

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Prometheus Metrics
app.UseMetricServer();
app.UseHttpMetrics();

// Rate Limiting
app.UseIpRateLimiting();

app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Apply migrations on startup
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        context.Database.Migrate();
        Console.WriteLine("Database migrations applied successfully.");
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while migrating the database.");
    }
}

app.Run();

// Make the implicit Program class public so test projects can access it
public partial class Program { }
