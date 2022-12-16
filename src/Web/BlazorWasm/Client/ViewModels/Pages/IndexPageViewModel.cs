using System.Net.Http.Json;
using System.Reactive.Linq;
using System.Windows.Input;
using LazyProperty;
using Microsoft.AspNetCore.Components;
using RankOverlay.Configurations;
using RankOverlay.RankProviders.ScoreSaber.ApiEntities;
using Reactive.Bindings;

namespace RankOverlay.Web.BlazorWasm.Client.ViewModels.Pages;

public class IndexPageViewModel : PageViewModelBase
{
    private HttpClient HttpClient { get; }
    public ILogger<IndexPageViewModel> Logger { get; }

    public IndexPageViewModel(
        HttpClient httpClient,
        ILogger<IndexPageViewModel> logger)
    {
        this.HttpClient = httpClient;
        this.Logger = logger;
    }

    public async Task InitializeAsync()
    {
        try
        {
            var globalConfig = await this.HttpClient
                .GetFromJsonAsync<GlobalConfigurations>("api/configurations/global");

            globalConfig ??= new GlobalConfigurations();

            if (globalConfig.DefaultPlayerId is not null)
            {
                var playerProfile = await this.HttpClient.GetFromJsonAsync<PlayerProfile>(
                        $"api/players/{globalConfig.DefaultPlayerId}");

                if (playerProfile is not null)
                {
                    this.PlayerId.Value = playerProfile.PlatformProfiles
                        .GetValueOrDefault(RankPlatformId.WellKnownPlatforms.ScoreSaber)
                        ?.PlatformPlayerId
                        ?? string.Empty;

                    this.PlayerProfile.Value = playerProfile;
                }
            }
        }
        catch (Exception ex)
        {
            this.Logger.LogError(ex, "Failed to load default config.");
        }

        if (this.PlayerId.Value is not null)
        {
            this.GetPlayerCommand.Execute(null);
        }
    }

    #region Bindings

    public ReactiveProperty<RankPlatformId> PlatformId => this.LazyReactiveProperty(RankPlatformId.WellKnownPlatforms.ScoreSaber);
    public ReactiveProperty<string?> PlayerId => this.LazyReactiveProperty((string?)null);
    public ReactiveProperty<PlayerProfile?> PlayerProfile => this.LazyReactiveProperty((PlayerProfile?)null);
    public ReactiveProperty<Player?> Player => this.LazyReactiveProperty((Player?)null);

    #endregion

    #region ICommands

    public ICommand SetDefaultPlayerIdCommand => this.LazyReactiveCommand(
        this.PlayerProfile.Select(x => !string.IsNullOrWhiteSpace(x?.Id)),
        async () =>
        {
            try
            {
                var playerId = this.PlayerProfile.Value?.Id;
                if (playerId is not null)
                {
                    await this.HttpClient.PutAsJsonAsync(
                        "api/configurations/global",
                        new
                        {
                            DefaultPlayerId = playerId
                        });
                }
            }
            catch (Exception ex)
            {
                this.Logger.LogError(ex, "Failed to set default player Id");
            }
        });

    public ICommand GetPlayerCommand => this.LazyReactiveCommand(
        this.PlayerId.Select(x => !string.IsNullOrWhiteSpace(x)),
        async () =>
        {
            try
            {
                if (string.IsNullOrWhiteSpace(this.PlayerProfile.Value?.Id))
                {
                    using var response = await this.HttpClient.PostAsJsonAsync(
                        $"/api/players",
                        new
                        {
                            ScoreSaberId = this.PlayerId.Value,
                        });

                    response.EnsureSuccessStatusCode();

                    var playerProfile = await response.Content
                        .ReadFromJsonAsync<PlayerProfile>();

                    if (playerProfile is null)
                    {
                        throw new InvalidOperationException("Failed to create a palyer profile.");
                    }

                    this.PlayerProfile.Value = playerProfile;
                    this.Logger.LogInformation("Player profile successfully created. Player ID: {PlayerId}", playerProfile.Id);
                }
                else
                {
                    var palyerProfile = await this.HttpClient.GetFromJsonAsync<PlayerProfile>(
                        $"/api/players/{this.PlayerProfile.Value.Id}");

                    if (palyerProfile is not null && this.NavigationManager is not null)
                    {
                        this.NavigationManager.NavigateTo(
                            this.NavigationManager.GetUriWithQueryParameter("playerId", palyerProfile.Id),
                            replace: true);
                    }

                    this.PlayerProfile.Value = palyerProfile;
                }
            }
            catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                this.Logger.LogInformation("User profile not found. Creating a new one.");

            }
            catch (Exception ex)
            {
                this.Logger.LogError(ex, "Failed to get player info.");
                return;
            }

            try
            {
                var platformId = RankPlatformId.WellKnownPlatforms.ScoreSaber;
                var playerId = this.PlayerProfile.Value?.Id;

                if (playerId is not null)
                {
                    this.Player.Value = await this.HttpClient.GetFromJsonAsync<Player>(
                        $"/api/players/{playerId}/platforms/{platformId}/profile");
                }
            }
            catch (Exception ex)
            {
                this.Logger.LogError(ex, "Failed to get ScoreSaber player info.");
            }
        });

    #endregion
}
