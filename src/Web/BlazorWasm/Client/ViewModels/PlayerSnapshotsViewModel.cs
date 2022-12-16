using System.Net.Http.Json;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Windows.Input;
using FluentUri;
using LazyProperty;
using RankOverlay.Configurations;
using RankOverlay.RankProviders.ScoreSaber;
using RankOverlay.RankProviders.ScoreSaber.ApiEntities;
using RankOverlay.Web.BlazorWasm.Client.ViewModels.Pages;
using Reactive.Bindings;

namespace RankOverlay.Web.BlazorWasm.Client.ViewModels;

public class PlayerSnapshotsViewModel : PageViewModelBase
{
    public PlayerProfile PlayerProfile { get; }
    public Player Player { get; }
    private HttpClient HttpClient { get; }
    private RankPlatformId PlatformId { get; } = RankPlatformId.WellKnownPlatforms.ScoreSaber;
    public ILogger<PlayerSnapshotsViewModel> Logger { get; }

    public PlayerSnapshotsViewModel(
        Player player,
        PlayerProfile playerProfile,
        HttpClient httpClient,
        ILogger<PlayerSnapshotsViewModel> logger)
    {
        ReactivePropertyScheduler.SetDefault(CurrentThreadScheduler.Instance);

        this.Player = player;
        this.HttpClient = httpClient;
        this.Logger = logger;
        this.PlayerProfile = playerProfile;
    }

    public async Task InitializeAsync()
    {
        _ = await this.UpdateDefaultSnapshotAsync();

        await this.UpdateSnapshotListAsync();
    }

    private async Task UpdateSnapshotListAsync()
    {
        var snapshots = await this.HttpClient.GetFromJsonAsync<ScoreSaberPlayerSnapshot[]>(
            $"/api/players/{this.PlayerProfile.Id}/platforms/{this.PlatformId}/snapshots");

        if (snapshots == null)
        {
            this.Logger.LogError("Got null from all snapshots endpoint.");
            return;
        }

        this.Snapshots.Clear();
        this.Snapshots.AddRangeOnScheduler(snapshots);
    }

    private async Task DeleteSnapshotAsync(ScoreSaberPlayerSnapshot snapshot)
    {
        await this.HttpClient.DeleteAsync($"/api/players/{snapshot.PlayerId}/platforms/{snapshot.PlatformId}/snapshots/{snapshot.Id}");
        this.Snapshots.Remove(snapshot);
    }

    private async Task SetAsDefaultSnapshotAsync(ScoreSaberPlayerSnapshot snapshot)
    {
        using var response = await this.HttpClient.PutAsJsonAsync(
            $"api/configurations/players/{snapshot.PlayerId}/",
            new
            {
                snapshot.PlayerId,
                DefaultPlatformId = snapshot.PlatformId,
                DefaultSnapshotId = snapshot.Id,
            });

        response.EnsureSuccessStatusCode();

        var playerConfig = await response.Content.ReadFromJsonAsync<PlayerConfigurations>();
        playerConfig ??= new PlayerConfigurations
        {
            PlayerId = snapshot.PlayerId,
        };

        await this.UpdatePlayerConfigurationsAsync(playerConfig);
    }

    private async Task<PlayerConfigurations> UpdatePlayerConfigurationsAsync(
        PlayerConfigurations? playerConfigurations = null)
    {
        var playerId = this.PlayerProfile.Id;

        playerConfigurations ??= await this.HttpClient.GetFromJsonAsync<PlayerConfigurations>(
            $"api/configurations/players/{playerId}");

        playerConfigurations ??= new PlayerConfigurations
        {
            PlayerId = playerId,
        };

        this.PlayerConfigurations.Value = playerConfigurations;

        this.DefaultSnapshot.Value = this.Snapshots
            .FirstOrDefault(x => x.Id == playerConfigurations.DefaultSnapshotId);

        return playerConfigurations;
    }

    private async Task<ScoreSaberPlayerSnapshot?> UpdateDefaultSnapshotAsync()
    {
        try
        {
            var playerConfig = await this.UpdatePlayerConfigurationsAsync();

            var defaultSnapshotId = playerConfig.DefaultSnapshotId;
            if (defaultSnapshotId is not null)
            {
                var snapshot = await this.HttpClient.GetFromJsonAsync<ScoreSaberPlayerSnapshot>(
                    $"api/players/{this.PlayerProfile.Id}/platforms/{this.PlatformId}/snapshots/{defaultSnapshotId}");

                this.DefaultSnapshot.Value = snapshot;
                return snapshot;
            }
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            this.Logger.LogInformation(ex, "Default snapshot is not set.");
        }

        return null;
    }

    private async Task TakeSnapshotAndSetAsDefaultAsync()
    {
        var snapshot = await this.TakeSnapshotAsync().ConfigureAwait(false);
        if (snapshot != null)
        {
            await this.SetAsDefaultSnapshotAsync(snapshot);
        }
    }

    private async Task<ScoreSaberPlayerSnapshot> TakeSnapshotAsync()
    {
        var response = await this.HttpClient.PostAsJsonAsync(
            $"/api/players/{this.PlayerProfile.Id}/platforms/{this.PlatformId}/snapshots",
            new
            {
                Description = string.Empty,
            });

        response.EnsureSuccessStatusCode();

        var snapshot = await response.Content.ReadFromJsonAsync<ScoreSaberPlayerSnapshot>();

        await this.UpdateSnapshotListAsync();

        return snapshot
            ?? throw new InvalidOperationException("Null snapshot instance was returned.");
    }

    public Uri BuildOverlayUri(ScoreSaberPlayerSnapshot snapshot)
    {
        return this.NavigationManager is null
            ? throw new InvalidOperationException("NavigationManager is null.")
            : FluentUriBuilder
            .From(this.NavigationManager.BaseUri)
            .Path($"overlay/{snapshot.PlayerId}")
            .QueryParam("platformId", snapshot.PlatformId)
            .QueryParam("snapshotId", snapshot.Id)
            .ToUri();
    }

    #region Bindings
    public ReactiveProperty<Uri> DefaultExplicitOverlayUrl => this.LazyReactiveProperty(() =>
    {
        return this.DefaultSnapshot
            .Select(snapshot =>
            {
                var path = $"overlay/{this.PlayerProfile.Id}";
                if (snapshot is not null)
                {
                    path += "/" + snapshot.Id;
                }

                return FluentUriBuilder
                    .From(this.NavigationManager!.BaseUri)
                    .Path(path)
                    .ToUri();
            })
            .ToReactiveProperty<Uri>();
    });

    public ReactiveProperty<Uri> DefaultExplicitOverlayUrlWithControl => this.LazyReactiveProperty(() =>
    {
        return this.DefaultExplicitOverlayUrl
            .Select(uri =>
            {
                return FluentUriBuilder
                    .From(uri.ToString())
                    .QueryParam("showControl", true)
                    .ToUri();
            })
            .ToReactiveProperty<Uri>();
    });

    public ReactiveProperty<Uri> DefaultImplicitOverlayUrl => this.LazyReactiveProperty(() =>
    {
        var path = $"overlay/{this.PlayerProfile.Id}";

        var uri = FluentUriBuilder
            .From(this.NavigationManager!.BaseUri)
            .Path(path)
            .ToUri();

        return new ReactiveProperty<Uri>(uri);
    });

    public ReactiveProperty<Uri> DefaultImplicitOverlayUrlWithControl => this.LazyReactiveProperty(() =>
    {
        return this.DefaultImplicitOverlayUrl
            .Select(uri =>
            {
                return FluentUriBuilder
                    .From(uri.ToString())
                    .QueryParam("showControl", true)
                    .ToUri();
            })
            .ToReactiveProperty<Uri>();
    });

    public ReactiveProperty<PlayerConfigurations> PlayerConfigurations => this.LazyReactiveProperty(() => new ReactiveProperty<PlayerConfigurations>());
    public ReactiveProperty<ScoreSaberPlayerSnapshot?> DefaultSnapshot => this.LazyReactiveProperty((ScoreSaberPlayerSnapshot?)default);
    public ReactiveCollection<ScoreSaberPlayerSnapshot> Snapshots => this.LazyReactiveCollection(() => new ReactiveCollection<ScoreSaberPlayerSnapshot>());
    #endregion

    #region ICommands

    public ICommand TakeSnapshotCommand => this.LazyAsyncReactiveCommand(async () =>
    {
        try
        {
            await this.TakeSnapshotAsync();
        }
        catch (Exception ex)
        {
            this.Logger.LogError(ex, "Failed to take a new snapshot.");
        }
    });

    public ICommand UpdateSnapshotsCommand => this.LazyAsyncReactiveCommand(async () =>
    {
        try
        {
            await this.UpdateSnapshotListAsync();
        }
        catch (Exception ex)
        {
            this.Logger.LogError(ex, "Failed to udpate snapshots.");
        }
    });

    public ICommand TakeSnapshotAndSetAsDefaultCommand => this.LazyAsyncReactiveCommand(async () =>
    {
        try
        {
            await this.TakeSnapshotAndSetAsDefaultAsync();
        }
        catch (Exception ex)
        {
            this.Logger.LogError(ex, "Faield to take a snapshot.");
        }
    });

    public ICommand DeleteSnapshotCommand => this.LazyAsyncReactiveCommand<ScoreSaberPlayerSnapshot>(async snapshot =>
    {
        try
        {
            await this.DeleteSnapshotAsync(snapshot);
        }
        catch (Exception ex)
        {
            this.Logger.LogError(ex, "Failed to delete snapshot.");
        }
    });

    public ICommand SetDefaultSnapshotCommand => this.LazyAsyncReactiveCommand<ScoreSaberPlayerSnapshot>(async snapshot =>
    {
        try
        {
            await this.SetAsDefaultSnapshotAsync(snapshot);
        }
        catch (Exception ex)
        {
            this.Logger.LogError(ex, "Failed to set default snapshot.");
        }
    });
    #endregion
}

