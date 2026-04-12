using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Plan2Gather.Components;
using Plan2Gather.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents().AddInteractiveServerComponents();
builder.Services.AddRazorPages();
builder.Services.AddDbContextFactory<Plan2GatherContext>(opts => opts.UseSqlite(builder.Configuration.GetConnectionString("Plan2GatherContext") ?? throw new NullReferenceException("Missing connection string")));
builder.Services.AddDbContext<ApplicationDbContext>(opts => opts.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.")));
builder.Services.AddQuickGridEntityFrameworkAdapter();
builder.Services.AddDatabaseDeveloperPageExceptionFilter();
builder.Services.AddDefaultIdentity<IdentityUser>(opts => opts.SignIn.RequireConfirmedAccount = true).AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddControllersWithViews();

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
app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();
app.MapStaticAssets();
app.MapRazorComponents<App>().AddInteractiveServerRenderMode();
app.MapRazorPages().WithStaticAssets();
app.Run();
