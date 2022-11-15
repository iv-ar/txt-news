global using I2R.LightNews.Services;
global using I2R.LightNews.Models;
global using IOL.Helpers;
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpClient();
builder.Services.AddScoped<GrabberService>();
builder.Services.AddRazorPages();

var app = builder.Build();

CultureInfo.CurrentCulture = new CultureInfo("nb-no");
CultureInfo.CurrentUICulture = new CultureInfo("nb-no");
app.UseStaticFiles();
app.UseStatusCodePages();
app.UseRouting();
app.MapRazorPages();
app.Run();