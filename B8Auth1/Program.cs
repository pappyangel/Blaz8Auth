using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.MicrosoftAccount;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using B8Auth1.Components;
using B8Auth1.Components.Account;
using B8Auth1.Data;

var builder = WebApplication.CreateBuilder(args);

var configBuilder = new ConfigurationBuilder()
                         .AddJsonFile("appsettings.json", true, true)
                         .AddJsonFile($"appsettings.Development.json", true, true)
                         .AddEnvironmentVariables()
                         .AddUserSecrets<Program>();

IConfiguration configuration = configBuilder.Build();
// var myClientSecret = configuration["MICROSOFT_CLIENT_SECRET"];
// var myClientId = configuration["MICROSOFT_CLIENTID"];

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<IdentityUserAccessor>();
builder.Services.AddScoped<IdentityRedirectManager>();
builder.Services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();


builder.Services.AddAuthentication(options =>
{
//    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
//    options.DefaultChallengeScheme = MicrosoftAccountDefaults.AuthenticationScheme;
    options.DefaultScheme = IdentityConstants.ApplicationScheme;
    options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
}) 
.AddMicrosoftAccount(microsoftOptions =>
{

   microsoftOptions.ClientId = configuration["MICROSOFT_CLIENTID"]!;
   microsoftOptions.ClientSecret = configuration["MICROSOFT_CLIENT_SECRET"]!;
})
.AddIdentityCookies();

var connectionString = configuration["AUTHDB_CONNECTION_STRING"]! ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));    
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddIdentityCore<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders();

builder.Services.AddSingleton<IEmailSender<ApplicationUser>, IdentityNoOpEmailSender>();

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("MustBePremiumUser",
    policy => policy.RequireClaim("IsPremiumUser", "true"));

    options.AddPolicy("MustBeAdminUser",
    policy => policy.RequireClaim("AdminUser", "true"));


});  //PremiumUser or User is Premium

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
    app.UseHttpsRedirection();
}



app.UseStaticFiles();


app.UseAuthentication();
app.UseAuthorization();

app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// Add additional endpoints required by the Identity /Account Razor components.
app.MapAdditionalIdentityEndpoints();

app.Run();
