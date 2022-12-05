global using I2R.LightNews.Services;
global using I2R.LightNews.Models;
global using IOL.Helpers;
using I2R.LightNews;
using I2R.LightNews.Services.Hosted;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpClient<NrkRadioService>();
builder.Services.AddHttpClient<NrkNewsService>();
builder.Services.AddMemoryCache();
builder.Services.AddScoped<NrkNewsService>();
builder.Services.AddSingleton<NrkRadioService>();
// builder.Services.AddHostedService<RadioIndexerService>();
builder.Services.AddControllers();
builder.Services.AddRazorPages().AddRazorRuntimeCompilation();

var app = builder.Build();

app.UseStaticFiles();
app.UseStatusCodePages();
app.UseRouting();
app.MapRazorPages();
app.MapControllers();
Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;
RadioIndexDb.CreateIfNotExists();
app.Run();