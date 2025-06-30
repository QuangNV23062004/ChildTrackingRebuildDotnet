using System.Text;
using CloudinaryDotNet;
using DotNetEnv;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MongoDB.Driver;
using RestAPI.Middlewares;
using RestAPI.Repositories.database;
using RestAPI.Repositories.interfaces;
using RestAPI.Repositories.repositories;
using RestAPI.Services.interfaces;
using RestAPI.Services.services;

var builder = WebApplication.CreateBuilder(args);

// Load environment variables
Env.Load();

// Add services to DI container
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IChildRepository, ChildRepository>();
builder.Services.AddScoped<IGrowthDataRepository, GrowthDataRepository>();
builder.Services.AddScoped<IGrowthMetricForAgeRepository, GrowthMetricsForAgeRepository>();
builder.Services.AddScoped<IGrowthVelocitoryRepository, GrowthVelocityRepository>();
builder.Services.AddScoped<IWflhRepository, WflhRepository>();
builder.Services.AddScoped<IRequestRepository, RequestRepository>();
builder.Services.AddScoped<IConsultationRepository, ConsultationRepository>();
builder.Services.AddScoped<IConsultationMessageRepository, ConsultationMessageRepository>();
builder.Services.AddScoped<IBlogRepository, BlogRepository>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IChildService, ChildService>();
builder.Services.AddScoped<IGrowthDataService, GrowthDataService>();
builder.Services.AddScoped<IRequestService, RequestService>();
builder.Services.AddScoped<IConsultationService, ConsultationService>();
builder.Services.AddScoped<IConsultationMessageService, ConsultationMessageService>();
builder.Services.AddScoped<IBlogService, BlogService>();

// MongoDB Configuration
builder.Services.Configure<MongoDBSettings>(options =>
{
    options.ConnectionString =
        Environment.GetEnvironmentVariable("DATABASE_URI")
        ?? throw new InvalidOperationException("DATABASE_URI not configured");
    ;
    options.DatabaseName =
        Environment.GetEnvironmentVariable("DATABASE_NAME")
        ?? throw new InvalidOperationException("DATABASE_NAME not configured");
    ;
});

// Register IMongoClient as a singleton to reuse across repositories
builder.Services.AddSingleton<IMongoClient>(sp =>
{
    var settings = sp.GetRequiredService<IOptions<MongoDBSettings>>().Value;
    return new MongoClient(settings.ConnectionString);
});

// Register IMongoDatabase as a singleton (optional, for cleaner repository initialization)
builder.Services.AddSingleton<IMongoDatabase>(sp =>
{
    var client = sp.GetRequiredService<IMongoClient>();
    var settings = sp.GetRequiredService<IOptions<MongoDBSettings>>().Value;
    return client.GetDatabase(settings.DatabaseName);
});

// Cloudinary Configuration

var cloudName =
    Environment.GetEnvironmentVariable("CLOUDINARY_CLOUD_NAME")
    ?? throw new InvalidOperationException("CLOUDINARY_CLOUD_NAME not configured");
var apiKey =
    Environment.GetEnvironmentVariable("CLOUDINARY_API_KEY")
    ?? throw new InvalidOperationException("CLOUDINARY_API_KEY not configured");
var apiSecret =
    Environment.GetEnvironmentVariable("CLOUDINARY_API_SECRET")
    ?? throw new InvalidOperationException("CLOUDINARY_API_SECRET not configured");

var account = new Account(cloudName, apiKey, apiSecret);
var cloudinary = new Cloudinary(account);

builder.Services.AddSingleton<ICloudinary>(cloudinary);

// Controllers and Swagger
builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();

// Swagger configuration with JWT support
builder.Services.AddSwaggerGen(opt =>
{
    opt.SwaggerDoc("v1", new OpenApiInfo { Title = "MyAPI", Version = "v1" });
    opt.AddSecurityDefinition(
        "Bearer",
        new OpenApiSecurityScheme
        {
            In = ParameterLocation.Header,
            Description = "Please enter token",
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            BearerFormat = "JWT",
            Scheme = "bearer",
        }
    );
    opt.AddSecurityRequirement(
        new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer",
                    },
                },
                new string[] { }
            },
        }
    );
});

// JWT Authentication Configuration
builder
    .Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        // Validate environment variables
        var issuer =
            Environment.GetEnvironmentVariable("JWT_ISSUER")
            ?? throw new InvalidOperationException("JWT_ISSUER not configured");

        var audience =
            Environment.GetEnvironmentVariable("JWT_AUDIENCE")
            ?? throw new InvalidOperationException("JWT_AUDIENCE not configured");

        var secret =
            Environment.GetEnvironmentVariable("JWT_SECRET")
            ?? throw new InvalidOperationException("JWT_SECRET not configured");

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = issuer,
            ValidateAudience = true,
            ValidAudience = audience,
            ValidateLifetime = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret)),
        };
    });

// CORS configuration
builder.Services.AddCors(options =>
{
    options.AddPolicy(
        "AllowAll",
        policy =>
        {
            policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
        }
    );
});

var app = builder.Build();

// Configure Swagger in all environments
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API v1");
    c.ConfigObject.AdditionalItems.Add("persistAuthorization", "true");
});

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}
app.UseCors("AllowAll");
app.UseHttpsRedirection();
app.UseMiddleware<ErrorHandlingMiddleware>();
app.UseRouting();

// Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<UserInfoMiddleware>();

app.MapControllers();

app.Run();
