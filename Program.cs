using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Plan2Gather.Auth;
using Plan2Gather.Components;
using Plan2Gather.Data;
using Plan2Gather.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents().AddInteractiveServerComponents();
builder.Services.AddRazorPages();

// DATABASE
builder.Services.AddDbContextFactory<Plan2GatherContext>(opts => opts.UseSqlite(builder.Configuration.GetConnectionString("Plan2GatherContext") ?? throw new NullReferenceException("Missing connection string")));

builder.Services.AddQuickGridEntityFrameworkAdapter();
builder.Services.AddDatabaseDeveloperPageExceptionFilter();
builder.Services.AddControllersWithViews();
builder.Services.AddScoped<ApiHttpClientFactory>();

// JWT
var jwtSecret = builder.Configuration["Jwt:Secret"]
    ?? throw new InvalidOperationException("Jwt:Secret is required in configuration.");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opts =>
    {
        opts.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "Plan2Gather",
            ValidAudience = builder.Configuration["Jwt:Audience"] ?? "Plan2Gather",
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

// Auth services
builder.Services.AddScoped<JwtService>();
builder.Services.AddScoped<JwtAuthStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(sp => sp.GetRequiredService<JwtAuthStateProvider>());

// HttpClient for Blazor components calling local API endpoints
builder.Services.AddHttpClient("API", client =>{});

// Notification service (in-memory for demo)
builder.Services.AddSingleton<NotificationService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope()) SeedData.Initialize(scope.ServiceProvider);

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment()) app.UseMigrationsEndPoint();
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
    app.UseMigrationsEndPoint();
}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
// Only enable HTTPS redirection when not in Development to avoid
// the "Failed to determine the https port for redirect" warning during dev.
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();
app.MapStaticAssets();
app.MapAuthEndpoints();
app.MapRazorComponents<App>().AddInteractiveServerRenderMode();
app.MapRazorPages().WithStaticAssets();
app.Run();
