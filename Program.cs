using choir_music_system.Data;
using choir_music_system.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.HttpOverrides;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();
builder.Services.AddSingleton<IPasswordHasher<object>, PasswordHasher<object>>();
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddPolicy("EmergencyLogin", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey:
                httpContext.Connection.RemoteIpAddress?.ToString()
                ?? "unknown",

            factory: _ =>
                new FixedWindowRateLimiterOptions
                {
                    PermitLimit = 5,
                    Window = TimeSpan.FromMinutes(1),
                    QueueLimit = 0
                }));
});

builder.Services.AddScoped<PdfMetadataExtractorService>();
builder.Services.AddScoped<PdfMergeService>();
builder.Services.AddScoped<PowerPointService>();

builder.Services.AddDbContext<ChoirDbContext>(options =>
    options.UseSqlite(
        builder.Configuration.GetConnectionString("ChoirDatabase")
    )
);

// Authentication
builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme =
            CookieAuthenticationDefaults.AuthenticationScheme;

        options.DefaultSignInScheme =
            CookieAuthenticationDefaults.AuthenticationScheme;

        options.DefaultChallengeScheme =
            CookieAuthenticationDefaults.AuthenticationScheme;
    })

    // Main Choir Music System login cookie
    .AddCookie(
        CookieAuthenticationDefaults.AuthenticationScheme,
        options =>
        {
            options.LoginPath = "/Account/Login";
            options.AccessDeniedPath = "/Account/AccessDenied";

            options.Cookie.Name = "ChoirMusic.Auth";
            options.Cookie.HttpOnly = true;
            options.Cookie.SameSite = SameSiteMode.Lax;
            options.Cookie.SecurePolicy = CookieSecurePolicy.Always;

            options.SlidingExpiration = true;
            options.ExpireTimeSpan = TimeSpan.FromHours(8);
        })

    // Temporary cookie used while Google login is being validated
    .AddCookie("External", options =>
    {
        options.Cookie.Name = "ChoirMusic.External";
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;

        options.ExpireTimeSpan = TimeSpan.FromMinutes(10);
    })

    .AddGoogle(
        GoogleDefaults.AuthenticationScheme,
        options =>
        {
            options.SignInScheme = "External";

            options.ClientId =
                builder.Configuration["Authentication:Google:ClientId"]
                ?? throw new InvalidOperationException(
                    "Google Client ID is not configured.");

            options.ClientSecret =
                builder.Configuration["Authentication:Google:ClientSecret"]
                ?? throw new InvalidOperationException(
                    "Google Client Secret is not configured.");
        });

// Authorization
builder.Services.AddAuthorization(options =>
{
    // Require authentication everywhere unless a page explicitly
    // allows anonymous access.
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();

    options.AddPolicy("AdminOnly", policy =>
        policy.RequireRole("Admin"));
});

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders =
        ForwardedHeaders.XForwardedFor |
        ForwardedHeaders.XForwardedProto;

    // Nginx Proxy Manager is outside the application container.
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

var app = builder.Build();

app.UseForwardedHeaders();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ChoirDbContext>();
    db.Database.Migrate();
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}


app.UseHttpsRedirection();

app.Use(async (context, next) =>
{
    context.Response.Headers["X-Content-Type-Options"] = "nosniff";
    context.Response.Headers["X-Frame-Options"] = "DENY";
    context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
    context.Response.Headers["Permissions-Policy"] =
        "camera=(), microphone=(), geolocation=()";

    await next();
});

app.UseRouting();


// Authentication MUST come before authorization.
app.UseAuthentication();
app.UseAuthorization();

app.UseRateLimiter();

app.MapStaticAssets()
   .AllowAnonymous();

app.MapRazorPages()
   .WithStaticAssets();

app.Run();