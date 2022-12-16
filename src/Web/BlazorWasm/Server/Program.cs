using System.Text;
using Microsoft.Extensions.Options;
using RankOverlay;
using RankOverlay.Configurations;
using RankOverlay.JsonConverters;
using RankOverlay.RankProviders.ScoreSaber;
using RankOverlay.Scores.Snapshots;
using RankOverlay.Web.BlazorWasm.Server.BlobStorage;
using Storage.Net;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services
    .AddControllersWithViews()
    .AddJsonOptions(o =>
    {
        o.JsonSerializerOptions.Converters.Add(new ConcreteClassConverter<PlayerScoreSnapshot, ScoreSaberPlayerSnapshot>());
    });

builder.Services.AddRazorPages();

builder.Services
    .AddHttpClient()
    .AddHttpClient(
        ScoreSaberApiClient.HttpClientName,
        client =>
        {
            client.BaseAddress = new Uri("https://scoresaber.com/");
        });

builder.Services
    .Configure<BlobStorageOptions>(builder.Configuration.GetSection("Storage:Blob"))
    .PostConfigure<BlobStorageOptions>(o =>
    {
        if (string.IsNullOrWhiteSpace(o.BaseDirectory))
        {
            o.BaseDirectory = Path.Combine(
                builder.Environment.ContentRootPath,
                ".database");
        }

        o.TextEncoding ??= Encoding.UTF8;
        o.TimeZoneOffset ??= TimeSpan.FromHours(9);

        o.JsonSerializerOptions ??= new System.Text.Json.JsonSerializerOptions
        {
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true,
        };
    });

builder.Services.AddSingleton(sp =>
{
    var logger = sp.GetRequiredService<ILogger<Program>>();
    var options = sp.GetRequiredService<IOptions<BlobStorageOptions>>().Value;

    logger.LogInformation("Data Directory: {DataDirectory}", options.BaseDirectory);
    return StorageFactory.Blobs.DirectoryFiles(options.BaseDirectory);
});

builder.Services.AddScoped<IPlayerScoreSnapshotRepositoryFactory, BlobStorageScoreSnapshotRepositoryFactory>();
builder.Services.AddScoped<IPlatformPlayerOperations, ScoreSaberPlatformPlayerOperations>();
builder.Services.AddScoped<IConfigurationStorage, BlobConfigurationStorage>();
builder.Services.AddScoped<IPlayerRepository, BlobPlayerRepository>();

builder.Services.AddScoped<IScoreSaberApiClient, ScoreSaberApiClient>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    //app.UseHsts();
}

//app.UseHttpsRedirection();

app.UseBlazorFrameworkFiles();
app.UseStaticFiles();

app.UseRouting();

app.MapRazorPages();
app.MapControllers();
app.MapFallbackToFile("index.html");

app.Run();
