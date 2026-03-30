using FoodSafety.MVC.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Serilog;
//Testing
using Microsoft.Extensions.FileProviders;


var builder = WebApplication.CreateBuilder(args);

// --- SERILOG SETUP ---
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()    
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Application", "FoodSafetyTracker")
    .Enrich.WithEnvironmentName()
    .WriteTo.Console()
    .WriteTo.File("Logs/log-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// --- DB & IDENTITY ---
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddControllersWithViews();


var app = builder.Build();

app.UseSerilogRequestLogging();

// Configure the HTTP request pipeline.
//Global error handling and environment-specific middleware
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");    
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

//Testing
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
        Path.Combine(builder.Environment.ContentRootPath, "Logs")),
    RequestPath = "/Logs"
});

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// Log user identity for every request
app.Use(async (context, next) =>
{
    var userName = context.User.Identity?.IsAuthenticated == true ? context.User.Identity.Name : "Anonymous";
    using (Serilog.Context.LogContext.PushProperty("UserName", userName))
    {
        await next.Invoke();
    }
});

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

// --- DATABASE INITIALIZATION SECTION ---
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();

        // Ensure DB exists
        context.Database.EnsureCreated();

        // Seed your Premises and Inspections
        await DbInitializer.SeedData(context);
        
        // We pass 'services' because this method needs RoleManager and UserManager
        await DbInitializer.SeedRolesAndAdmin(services);

        Log.Information("Database and Roles initialized successfully.");
    }
    catch (Exception ex)
    {
        Log.Error(ex, "An error occurred during database initialization.");
    }
}

app.Run();
