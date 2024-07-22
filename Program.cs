using FoodDelivery.Models;
using FoodDelivery.RabbitMQ;
using FoodDelivery.Services;
using Microsoft.AspNetCore.Identity;
using MongoDB.Driver;
using FoodDelivery.Helpers;
using FoodDelivery.MongoDB;
using Microsoft.AspNetCore.Authentication.Cookies;
using MongoDB.Bson;
using DotNetEnv;

var builder = WebApplication.CreateBuilder(args);

Env.Load("secret.env");

var unsplashAccessKey = Environment.GetEnvironmentVariable("UNSPLASH_ACCESS_KEY");
var cloudinaryCloudName = Environment.GetEnvironmentVariable("CLOUDINARY_CLOUD_NAME");
var cloudinaryApiKey = Environment.GetEnvironmentVariable("CLOUDINARY_API_KEY");
var cloudinaryApiSecret = Environment.GetEnvironmentVariable("CLOUDINARY_API_SECRET");

builder.Configuration["Unsplash:AccessKey"] = unsplashAccessKey;
builder.Configuration["Cloudinary:CloudName"] = cloudinaryCloudName;
builder.Configuration["Cloudinary:ApiKey"] = cloudinaryApiKey;
builder.Configuration["Cloudinary:ApiSecret"] = cloudinaryApiSecret;

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
});

builder.Services.AddControllersWithViews();

builder.Services.Configure<MongoDbConfig>(builder.Configuration.GetSection(nameof(MongoDbConfig)));
var mongoSettings = builder.Configuration.GetSection(nameof(MongoDbConfig)).Get<MongoDbConfig>();

if (mongoSettings == null)
{
    throw new Exception("MongoDB settings are not configured properly.");
}

builder.Services.AddSingleton<IMongoClient, MongoClient>(sp =>
{
    return new MongoClient(mongoSettings.ConnectionString);
});

builder.Services.AddScoped(sp =>
{
    var client = sp.GetRequiredService<IMongoClient>();
    return client.GetDatabase(mongoSettings.Name);
});

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddSession();

builder.Services.AddIdentity<ApplicationUser, ApplicationRole>()
    .AddMongoDbStores<ApplicationUser, ApplicationRole, ObjectId>(
        mongoSettings.ConnectionString, mongoSettings.Name
    )
    .AddDefaultTokenProviders();

builder.Services.AddScoped<UserManager<ApplicationUser>>();
builder.Services.AddScoped<RoleManager<ApplicationRole>>();
builder.Services.AddScoped<SignInManager<ApplicationUser>>();

builder.Services.AddSingleton<ImageService>();

builder.Services.Configure<IdentityOptions>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 8;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;
    options.User.RequireUniqueEmail = true;
});

builder.Services.AddLogging(logging =>
{
    logging.ClearProviders();
    logging.AddConsole();
    logging.AddDebug();
});

builder.Services.AddSingleton<IRabbitMqService, RabbitMqService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        await IdentityDataInitializer.SeedRolesAndAdminUser(services);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database.");
    }
}

app.UseStatusCodePages(async context =>
{
    var response = context.HttpContext.Response;

    if (response.StatusCode == 404)
    {
        response.Redirect("/Error/NotFound");
    }
    else if (response.StatusCode == 401)
    {
        response.Redirect("/Error/Unauthorized");
    }
    else if (response.StatusCode == 403)
    {
        response.Redirect("/Error/Unauthorized");
    }
});

// Configure the HTTP request pipeline
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
app.UseSession();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
