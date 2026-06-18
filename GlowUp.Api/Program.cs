using System.Text.Json.Serialization;
using GlowUp.Api.Data;
using GlowUp.Api.Services;
using Microsoft.AspNetCore.HttpOverrides;
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
// The connection string is supplied at runtime from configuration:
//   - Local dev:  user-secrets  -> ConnectionStrings:DefaultConnection
//   - Azure:      App Service Connection String "DefaultConnection"
//                 (surfaces as ConnectionStrings__DefaultConnection)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));

// --- Application services ----------------------------------------------------
builder.Services.AddScoped<IProfileService, ProfileService>();
builder.Services.AddScoped<ISectionService, SectionService>();
builder.Services.AddScoped<ICustomSectionService, CustomSectionService>();
builder.Services.AddScoped<IAiContextService, AiContextService>();
builder.Services.AddScoped<IAiChatService, AiChatService>();
builder.Services.AddScoped<DemoDataSeeder>();

// AI provider selection (config-driven). Both concrete providers are registered;
// IAiProviderService resolves to Gemini only when configured with a key, otherwise
// to the mock. The Gemini provider itself also falls back to mock at runtime on error.
builder.Services.AddScoped<MockAiProviderService>();
builder.Services.AddScoped<GeminiAiProviderService>();
builder.Services.AddScoped<IAiProviderService>(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    var provider = config["AI:Provider"];
    var apiKey = config["AI:Gemini:ApiKey"];

    var useGemini = string.Equals(provider, "Gemini", StringComparison.OrdinalIgnoreCase)
                    && !string.IsNullOrWhiteSpace(apiKey);

    return useGemini
        ? sp.GetRequiredService<GeminiAiProviderService>()
        : sp.GetRequiredService<MockAiProviderService>();
});

// --- CORS --------------------------------------------------------------------
// In the single-URL deployment the frontend is served from the same origin, so
// CORS is not exercised. Origins are still configurable (Cors:AllowedOrigins) for
// the case where the frontend is hosted separately; defaults cover local dev.
var allowedOrigins = builder.Configuration
    .GetSection("Cors:AllowedOrigins")
    .Get<string[]>() ?? new[] { "http://localhost:5173", "http://localhost:3000" };

builder.Services.AddCors(options =>
{
    options.AddPolicy(FrontendCorsPolicy, policy =>
        policy.WithOrigins(allowedOrigins).AllowAnyHeader().AllowAnyMethod());
});

// --- Swagger / OpenAPI -------------------------------------------------------
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// --- Startup database tasks (opt-in via configuration) ----------------------
// Both default to false, so local development is unchanged. In Azure, enable them
// so the demo database is created and populated automatically on first boot.
await using (var scope = app.Services.CreateAsyncScope())
{
    var sp = scope.ServiceProvider;
    var config = sp.GetRequiredService<IConfiguration>();

    if (config.GetValue<bool>("Database:MigrateOnStartup"))
    {
        var db = sp.GetRequiredService<AppDbContext>();
        await db.Database.MigrateAsync();
    }

    if (config.GetValue<bool>("Demo:Seed"))
    {
        var seeder = sp.GetRequiredService<DemoDataSeeder>();
        await seeder.SeedAsync();
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    // Behind Azure App Service's reverse proxy: honor the forwarded scheme so
    // HTTPS redirection and link generation work correctly.
    app.UseForwardedHeaders(new ForwardedHeadersOptions
    {
        ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
    });

    // Clean, generic error response — never leaks exception details or secrets.
    app.UseExceptionHandler(handler =>
    {
        handler.Run(async context =>
        {
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsJsonAsync(new
            {
                error = "Something went wrong. Please try again."
            });
        });
    });
}

app.UseHttpsRedirection();

// Serve the built React app (wwwroot) for the single-URL deployment.
app.UseDefaultFiles();
app.UseStaticFiles();

app.UseCors(FrontendCorsPolicy);

app.UseAuthorization();

app.MapControllers();

// SPA fallback: any non-API, non-file route returns index.html so client-side
// routing (e.g. /custom-sections/...) works on a hard refresh.
app.MapFallbackToFile("index.html");

app.Run();
