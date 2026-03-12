using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Localization;
using System.Globalization;
using AspNet.Security.OAuth.GitHub;
using InventoryApp.Web.Data;
using InventoryApp.Web.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedProto;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

builder.Services.AddControllersWithViews();
builder.Services.AddLocalization();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<AppUser, IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.Events.OnValidatePrincipal = async context =>
    {
        var userManager = context.HttpContext.RequestServices.GetRequiredService<UserManager<AppUser>>();
        if (context.Principal == null)
            return;

        var userId = userManager.GetUserId(context.Principal);
        if (string.IsNullOrEmpty(userId))
            return;

        var user = await userManager.FindByIdAsync(userId);
        if (user?.IsBlocked == true)
        {
            context.RejectPrincipal();
            await context.HttpContext.SignOutAsync(IdentityConstants.ApplicationScheme);
        }
    };
});

var authBuilder = builder.Services.AddAuthentication();

var googleClientId = builder.Configuration["Auth:Google:ClientId"];
var googleClientSecret = builder.Configuration["Auth:Google:ClientSecret"];
if (!string.IsNullOrWhiteSpace(googleClientId) && !string.IsNullOrWhiteSpace(googleClientSecret))
{
    authBuilder.AddGoogle(options =>
    {
        options.ClientId = googleClientId!;
        options.ClientSecret = googleClientSecret!;
    });
}

var gitHubClientId = builder.Configuration["Auth:GitHub:ClientId"];
var gitHubClientSecret = builder.Configuration["Auth:GitHub:ClientSecret"];
if (!string.IsNullOrWhiteSpace(gitHubClientId) && !string.IsNullOrWhiteSpace(gitHubClientSecret))
{
    authBuilder.AddGitHub(options =>
    {
        options.ClientId = gitHubClientId!;
        options.ClientSecret = gitHubClientSecret!;
    });
}

var app = builder.Build();

app.UseForwardedHeaders();

var supportedCultures = new[] { new CultureInfo("en"), new CultureInfo("ru") };
app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture("en"),
    SupportedCultures = supportedCultures,
    SupportedUICultures = supportedCultures,
    RequestCultureProviders = new IRequestCultureProvider[]
    {
        new CookieRequestCultureProvider()
    }
});

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();

    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    if (!await roleManager.RoleExistsAsync("Admin"))
    {
        await roleManager.CreateAsync(new IdentityRole("Admin"));
    }
}

app.Run();
