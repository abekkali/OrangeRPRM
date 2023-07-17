using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using RPRM.Data;
using RPRM.Data.ProgHub;
using RPRM.Models.User;
using RPRM.Services;
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySQL(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<User>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();
//policy
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Admin", policy => policy.RequireRole("Admin"));    options.AddPolicy("Manager", policy => policy.RequireRole("Manager"));
});
builder.Services.AddScoped<PermissionHandler>();
builder.Services.Configure<IdentityOptions>(options =>
{
    // Paramètres du mot de passe.
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequiredLength = 6; 
    options.Password.RequiredUniqueChars = 1;
});
var supportedCultures = new[] { new CultureInfo("en-US") };
builder.Services.Configure<RequestLocalizationOptions>(opts =>
{
    opts.DefaultRequestCulture = new RequestCulture("en-US"); 
    opts.SupportedCultures = supportedCultures;
    opts.SupportedUICultures = supportedCultures;
});
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
builder.Services.AddSignalR();
builder.Services.Configure<DataProtectionTokenProviderOptions>(o =>
       o.TokenLifespan = TimeSpan.FromHours(3));
builder.Services.AddTransient<IEmailSender, EmailSender>();
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(
        builder =>
        {
            builder.WithOrigins("https://localhost:7273/") // Votre origine ici
                .AllowAnyHeader()
                .AllowAnyMethod();
        });
});
var app = builder.Build();
app.UseCors(cors => cors
    .WithOrigins("https://localhost:7273/") // Votre origine ici
    .AllowAnyMethod()
    .AllowAnyHeader());
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
    endpoints.MapHub<ImportHub>("/importHub");
    endpoints.MapHub<ProgressHub>("/progressHub"); 
});
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();
