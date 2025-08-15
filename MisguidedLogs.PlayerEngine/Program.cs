using BunnyCDN.Net.Storage;
using MisguidedLogs.PlayerEngine;
using MisguidedLogs.PlayerEngine.Repositories;
using MisguidedLogs.PlayerEngine.Repositories.Bunnycdn;
using MisguidedLogs.PlayerEngine.Services;
using MisguidedLogs.PlayerEngine.Services.HostedService;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
}); 

builder.Services.Configure<ClientConfig>(builder.Configuration);
var config = builder.Configuration.GetSection("ClientEndpoint").Get<ClientConfig>();

ArgumentNullException.ThrowIfNull(config);
builder.Services.AddSingleton(config);

builder.Services.AddSingleton(new BunnyCDNStorage(config.BunnyCdnStorage, config.BunnyAccessKey, "se"));
builder.Services.AddSingleton<BunnyCdnStorageLoader>();
builder.Services.AddTransient<Searcher>();
builder.Services.AddTransient<ProbabilityService>();
builder.Services.AddSingleton(x => 
{
    var engine = new PlayerEngine(x.GetService<BunnyCdnStorageLoader>()!, x.GetService<ProbabilityService>()!);
    return engine;  
});
builder.Services.AddHostedService<Updater>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllOrigins",
        builder => builder.AllowAnyOrigin()
                          .AllowAnyMethod()
                          .AllowAnyHeader());
});

var app = builder.Build();
// Configure the HTTP request pipeline.

app.UseHttpsRedirection();

app.UseCors("AllowAllOrigins"); // Enable CORS

app.UseAuthorization();

app.MapControllers();

await app.RunAsync();
