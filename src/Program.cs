global using I2R.LightNews.Services;
global using I2R.LightNews.Models;
global using IOL.Helpers;
using System.Text.Json.Serialization;
using I2R.LightNews;
using Microsoft.AspNetCore.Http.Json;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpClient<NrkRadioService>();
builder.Services.AddHttpClient<NrkNewsService>();
builder.Services.AddMemoryCache();
builder.Services.AddScoped<NrkNewsService>();
builder.Services.AddScoped<NrkRadioService>();
builder.Services.Configure<JsonOptions>(options => {
    options.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.Never;
    options.SerializerOptions.PropertyNameCaseInsensitive = true;
});
builder.Services.AddControllers();
builder.Services.AddRazorPages().AddRazorRuntimeCompilation();

var app = builder.Build();

app.UseStaticFiles();
app.UseStatusCodePages();
app.UseRouting();
app.MapRazorPages();
app.MapControllers();
RadioIndexDb.CreateIfNotExists();
app.Run();