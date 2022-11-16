global using I2R.LightNews.Services;
global using I2R.LightNews.Models;
global using IOL.Helpers;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpClient();
builder.Services.AddMemoryCache();
builder.Services.AddScoped<GrabberService>();
builder.Services.AddRazorPages().AddRazorRuntimeCompilation();

var app = builder.Build();

app.UseStaticFiles();
app.UseStatusCodePages();
app.UseRouting();
app.MapRazorPages();
app.Run();