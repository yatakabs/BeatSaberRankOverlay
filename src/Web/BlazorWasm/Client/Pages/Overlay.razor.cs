using System.Globalization;
using System.Net.Http.Json;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Microsoft.AspNetCore.Components;
using RankOverlay.Configurations;
using RankOverlay.PlayerScores;
using RankOverlay.RankProviders.ScoreSaber;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace RankOverlay.Web.BlazorWasm.Client.Pages;

public partial class Overlay : IDisposable
{
    public CompositeDisposable Disposables { get; } = new CompositeDisposable();
    public Session ViewModelSession { get; } = new Session();

    #region Parameters

    [Parameter]
    public string PlayerId { get; set; } = string.Empty;

    [SupplyParameterFromQuery]
    [Parameter]
    public string? SnapshotId { get; set; }

    [SupplyParameterFromQuery(Name = "scroll")]
    [Parameter]
    public bool EnableScroll { get; set; } = false;

    [SupplyParameterFromQuery]
    [Parameter]
    public int RecentScoresCount { get; set; }

    [SupplyParameterFromQuery(Name = "updateInterval")]
    [Parameter]
    public double UpdateIntervalInSeconds { get; set; }

    [SupplyParameterFromQuery(Name = "culture")]
    [Parameter]
    public string? CultureName { get; set; }

    [SupplyParameterFromQuery]
    [Parameter]
    public bool ShowControl { get; set; }

    [SupplyParameterFromQuery]
    [Parameter]
    public string? PlatformId { get; set; }

    #endregion

    #region Bindings
    public ReactivePropertySlim<string> ControlMessage { get; } = new ReactivePropertySlim<string>();
    public ReactivePropertySlim<string?> ErrorMessage { get; } = new();
    public ReactivePropertySlim<OverlayViewModel?> OverlayVm { get; } = new();
    public ReactivePropertySlim<int> ImageCaptureWidth { get; } = new(1000);
    #endregion

    private CultureInfo Culture { get; set; } = CultureInfo.CurrentUICulture;

    public async Task CaptureImage()
    {
        this.ControlMessage.Value = "Capturing image...";
        try
        {
            _ = await this.JsRuntime.InvokeAsync<object>(
                "window.copyElementToClipboard",
                new object[] {
                    "overlay-container",
                    this.ImageCaptureWidth.Value
                });

            this.ControlMessage.Value = "Successfully captured image to clipboard.";
            this.ErrorMessage.Value = string.Empty;
        }
        catch (Exception ex)
        {
            this.Logger.LogError(ex, "Failed to capture image.");

            this.ControlMessage.Value = string.Empty;
            this.ErrorMessage.Value = "Failed to capture image.";
        }
    }

    protected override async Task OnParametersSetAsync()
    {
        //ReactivePropertyScheduler.SetDefault(CurrentThreadScheduler.Instance);

        var playerId = (PlayerId)this.PlayerId;

        if (string.IsNullOrWhiteSpace(this.PlatformId) || !RankPlatformId.TryParse(this.PlatformId, out var platformId))
        {
            platformId = RankPlatformId.WellKnownPlatforms.ScoreSaber;
            this.PlatformId = platformId.ToString();
        }

        var snapshotId = PlayerScoreSnapshotId.Empty;
        if (string.IsNullOrWhiteSpace(this.SnapshotId) || !PlayerScoreSnapshotId.TryParse(this.SnapshotId ?? string.Empty, out snapshotId))
        {
            var playerConfigurations = await this.HttpClient.GetFromJsonAsync<PlayerConfigurations>(
                $"api/configurations/players/{playerId}");

            if (playerConfigurations?.DefaultSnapshotId is not null)
            {
                snapshotId = playerConfigurations.DefaultSnapshotId.Value;
                this.SnapshotId = snapshotId.ToString(); ;

                this.Logger.LogInformation("No snapshot ID specified. Fall back to the default snapshot. Snapshot ID: {SnapshotId}", snapshotId.ToString());
            }
        }

        var vm = this.OverlayVm.Value;

        var shouldRecreateVm =
            vm == null ||
            vm.Snapshot.Id.ToString() != this.SnapshotId ||
            vm.PlayerId != this.PlayerId ||
            vm.UpdateInterval.TotalSeconds != this.UpdateIntervalInSeconds ||
            vm.PlatformId != platformId;

        if (shouldRecreateVm)
        {
            var updateInterval = this.UpdateIntervalInSeconds > 0
                ? TimeSpan.FromSeconds(this.UpdateIntervalInSeconds)
                : TimeSpan.FromSeconds(30);

            try
            {
                var snapshot = await this.GetSnapshotAsync(
                    playerId,
                    platformId,
                    snapshotId);

                if (snapshot == null)
                {
                    this.Logger.LogError("Snapshot is null");
                    this.ErrorMessage.Value = "Invalid snapshot detected.";
                    vm = this.OverlayVm.Value = null;
                }
                else
                {
                    this.OverlayVm.Value = vm = ActivatorUtilities.CreateInstance<OverlayViewModel>(
                        this.ServiceProvider,
                        playerId,
                        platformId,
                        snapshot,
                        updateInterval);
                }
            }
            catch (Exception ex)
            {
                this.Logger.LogError(ex, "Failed to udpate view model.");
                if (this.ErrorMessage.Value == null)
                {
                    this.ErrorMessage.Value = "Failed to update score information.";
                }
            }
            finally
            {
                this.StateHasChanged();
            }
        }

        if (vm != null)
        {
            vm.RecentScoresCount.Value = this.RecentScoresCount;
        }

        this.Culture = this.CultureName == null
            ? CultureInfo.CurrentUICulture
            : CultureInfo.GetCultureInfo(this.CultureName);
    }

    private async Task<ScoreSaberPlayerSnapshot?> GetSnapshotAsync(
        PlayerId playerId,
        RankPlatformId platformId,
        PlayerScoreSnapshotId snapshotId)
    {
        try
        {
            return await this.HttpClient.GetFromJsonAsync<ScoreSaberPlayerSnapshot>(
                $"/api/players/{playerId}/platforms/{platformId}/snapshots/{snapshotId}");
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            this.Logger.LogError(ex, $"No snapshot found. ID: {snapshotId}");
            this.ErrorMessage.Value = $"No snapshot found. ID: {snapshotId}";
            throw;
        }
        catch (Exception ex)
        {
            this.Logger.LogError(ex, "Failed to obtain the snapshot.");
            this.ErrorMessage.Value = "Failed to obtain the snapshot.";
            throw;
        }
    }

    protected override void OnInitialized()
    {
        var scheduler = CurrentThreadScheduler.Instance;
        this.OverlayVm
            .DisposePreviousValue()
            .Subscribe()
            .AddTo(this.Disposables);

        Observable
            .Concat(
                this.ImageCaptureWidth.PropertyChangedAsObservable(),
                this.ErrorMessage.PropertyChangedAsObservable(),
                this.ControlMessage.PropertyChangedAsObservable(),
                this.OverlayVm.PropertyChangedAsObservable())
            .ObserveOn(scheduler)
            .Subscribe(_ =>
            {
                this.StateHasChanged();
            })
            .AddTo(this.Disposables);

        this.OverlayVm
            .Subscribe(viewModel =>
            {
                if (viewModel is not null)
                {
                    this.ViewModelSession.StartNew(sessionDisposable =>
                    {
                        viewModel!
                            .ValueChanged
                            .ObserveOn(scheduler)
                            .Subscribe(x =>
                            {
                                this.StateHasChanged();
                            })
                            .AddTo(sessionDisposable);
                    });
                }
                else
                {
                    this.ViewModelSession.Stop();
                }
            });

        Observable
            .Interval(TimeSpan.FromMilliseconds(250))
            .ObserveOn(CurrentThreadScheduler.Instance)
            .Subscribe(_ =>
            {
                try
                {
                    this.StateHasChanged();
                }
                catch (Exception ex)
                {
                    this.Logger.LogError(ex, "Failed to notify state changes.");
                }
            })
            .AddTo(this.Disposables);
    }

    #region IDisposable

    private bool isDisposed = false;

    public void Dispose()
    {
        this.Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (this.isDisposed)
        { return; }

        if (disposing)
        {
            this.Disposables.Dispose();
        }

        this.isDisposed = true;
    }

    protected void ThrowIfDisposed()
    {
        if (this.isDisposed)
        {
            throw new ObjectDisposedException(this.GetObjectName());
        }
    }

    protected virtual string GetObjectName()
    {
        return this.ToString()!;
    }

    #endregion IDisposable
}
