using System.Text.Json.Serialization;
using GlowUp.Api.Data;
using GlowUp.Api.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

const string FrontendCorsPolicy = "AllowFrontend";

// --- Controllers + JSON options ---------------------------------------------
builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
    {
        // Serialize enums (e.g. SectionType) as their string names.
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

// --- Database (SQL Server via EF Core) --------------------------------------
// The connection string is supplied at runtime from configuration. For local
// development set it with .NET user-secrets so it is never committed:
//   ConnectionStrings:DefaultConnection
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));

// --- Application services ----------------------------------------------------
builder.Services.AddScoped<IProfileService, ProfileService>();
builder.Services.AddScoped<ISectionService, SectionService>();
builder.Services.AddScoped<ICustomSectionService, CustomSectionService>();
builder.Services.AddScoped<IAiContextService, AiContextService>();
builder.Services.AddScoped<IAiChatService, AiChatService>();
// Mock provider for now; swap for a real AI provider later without touching callers.
builder.Services.AddScoped<IAiProviderService, MockAiProviderService>();

// --- CORS (prepared for the future React frontend) ---------------------------
builder.Services.AddCors(options =>
{
    options.AddPolicy(FrontendCorsPolicy, policy =>
        policy
            // Common Vite (5173) and CRA (3000) dev server origins.
            .WithOrigins("http://localhost:5173", "http://localhost:3000")
            .AllowAnyHeader()
            .AllowAnyMethod());
});

// --- Swagger / OpenAPI -------------------------------------------------------
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors(FrontendCorsPolicy);

app.UseAuthorization();

app.MapControllers();

app.Run();
