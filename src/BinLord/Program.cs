using BinLord.Data;
using BinLord.Infrastructure;
using BinLord.Services;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews(options => options.Filters.Add<AppSettingsFilter>());
builder.Services.AddDbContext<BinLordContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("BinLordContext")));
builder.Services.AddScoped<SettingsService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<BinLordContext>();
    db.Database.Migrate();
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

// Trust X-Forwarded-* headers from any proxy so the scheme/host seen by the
// app (and used for RSS/calendar links) reflect the public-facing reverse
// proxy rather than the internal address Kestrel is bound to. This assumes
// BinLord is only reachable through that proxy, not directly from the
// internet — if it's directly exposed too, set a Base URL override in
// Settings instead of relying on these headers.
var forwardedHeadersOptions = new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedHost,
};
forwardedHeadersOptions.KnownNetworks.Clear();
forwardedHeadersOptions.KnownProxies.Clear();
app.UseForwardedHeaders(forwardedHeadersOptions);

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
