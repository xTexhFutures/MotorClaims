
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using MotorClaims.Models;
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
var appsetting = builder.Configuration.GetSection("AppSettings");
builder.Services.AddHttpContextAccessor();
builder.Services.Configure<AppSettings>(appsetting);
AppSettings appSettings = appsetting.Get<AppSettings>();

builder.Services.AddControllersWithViews();

builder.Services.AddMemoryCache();

builder.Services.AddRazorPages();

builder.Services.AddMvc(config =>
{
    config.Filters.Add(new LevelSecurity(appSettings));


});
builder.Services.AddHostedService<MyHostedService>();
builder.Services.AddSession(options => {
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.Name = ".AspNetCore.Session.SME";
    options.Cookie.IsEssential = false;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;


});
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");
builder.Services.AddMvc().AddViewLocalization(Microsoft.AspNetCore.Mvc.Razor.LanguageViewLocationExpanderFormat.Suffix);
builder.Services.Configure<RequestLocalizationOptions>(option =>
{
    var supportCulture = new[]
    {
                    new CultureInfo("en"),
                    new CultureInfo("ar-JO")
                };
    option.DefaultRequestCulture = new RequestCulture(culture: "ar-JO", uiCulture: "ar-JO");
    option.SupportedCultures = supportCulture;
    option.SupportedUICultures = supportCulture;
});
var app = builder.Build();


app.UseHttpsRedirection();

app.UseAuthentication();
var AppLocalization = app.Services.GetService<IOptions<RequestLocalizationOptions>>();
app.UseRequestLocalization(AppLocalization.Value);
app.MapControllers();
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();
//app.UseFileServer(new FileServerOptions
//{
//    FileProvider = new PhysicalFileProvider(@"D:/AICCFiles/AB0810232"),
//    RequestPath = new PathString("/"),
//    EnableDirectoryBrowsing = false
//});

app.UseFileServer(new FileServerOptions
{
    FileProvider = new PhysicalFileProvider(@"E:\NajmImages"),
    RequestPath = new PathString("/MyPath"),
    EnableDirectoryBrowsing = false
});
app.UseEndpoints(endpoints =>
{
    endpoints.MapControllerRoute(
        name: "default",
        pattern: "{controller=Authenticator}/{action=Login}/{id?}");
});
app.UseEndpoints(endpoints =>
{
    endpoints.MapControllerRoute(
        name: "api",
        pattern: "api/{controller=Home}/{action=Index}/{name?}");
});
app.Run();


