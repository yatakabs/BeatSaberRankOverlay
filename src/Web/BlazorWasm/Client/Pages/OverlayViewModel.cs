using System.Net.Http.Json;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using FluentUri;
using RankOverlay.RankProviders.ScoreSaber;
using RankOverlay.RankProviders.ScoreSaber.ApiEntities;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace RankOverlay.Web.BlazorWasm.Client.Pages;

public sealed class OverlayViewModel : IDisposable
{
    private CompositeDisposable Disposables { get; } = new CompositeDisposable();

    public PlayerId PlayerId { get; }
    public RankPlatformId PlatformId { get; }
    public TimeSpan UpdateInterval { get; }
    public ScoreSaberPlayerSnapshot Snapshot { get; }
    private ILogger Logger { get; }
    private HttpClient HttpClient { get; }

    #region Bindings
    public ReactivePropertySlim<int> RecentScoresCount { get; } = new();
    public ReactivePropertySlim<bool> IsUpdating { get; } = new();
    public ReactivePropertySlim<Player?> Player { get; } = new();
    public ReactiveCollection<PlayerScore> RecentScores { get; } = new();
    public ReactivePropertySlim<DateTimeOffset> LastUpdatedTime { get; } = new();
    #endregion

    public IObservable<Unit> ValueChanged { get; }

    public OverlayViewModel(
        PlayerId playerId,
        RankPlatformId platformId,
        ScoreSaberPlayerSnapshot snapshot,
        TimeSpan updateInterval,
        ILogger<OverlayViewModel> logger,
        HttpClient httpClient)
    {
        this.PlayerId = playerId;
        this.PlatformId = platformId;
        this.Snapshot = snapshot;
        this.UpdateInterval = updateInterval;
        this.Logger = logger;
        this.HttpClient = httpClient;

        this.ValueChanged = Observable.Merge(
            this.RecentScoresCount.Select(x => Unit.Default),
            this.IsUpdating.Select(x => Unit.Default),
            this.Player.Select(x => Unit.Default),
            this.RecentScores.CollectionChangedAsObservable().Select(x => Unit.Default));

        _ = Observable
            .Timer(TimeSpan.Zero, updateInterval)
            .ObserveOn(CurrentThreadScheduler.Instance)
            .Select(x => Observable.FromAsync(async cancellationToken =>
            {
                try
                {
                    this.IsUpdating.Value = true;

                    await Task.WhenAll(
                        this.UpdatePlayerAsync(cancellationToken),
                        this.UpdateRecentScoresAsync(cancellationToken));

                    this.LastUpdatedTime.Value = DateTimeOffset.Now;
                }
                catch (OperationCanceledException) { }
                catch (Exception ex)
                {
                    this.Logger.LogError(ex, "Failed to update overlay info.");
                }
                finally
                {
                    this.IsUpdating.Value = false;
                }
            }))
            .Merge(1)
            .Subscribe()
            .AddTo(this.Disposables);
    }

    private async Task UpdateRecentScoresAsync(
        CancellationToken cancellationToken = default)
    {
        var since = this.RecentScores.Count != 0
            ? this.RecentScores.Max(x => x.Score.TimeSet)
            : this.Snapshot.Timestamp;

        var uriBuilder = FluentUriBuilder.Create()
            .Path($"api/players/{this.Snapshot.PlayerId}/platforms/{this.Snapshot.PlatformId}/scores/recent/ranked")
            .QueryParam("since", since.ToString("O"));

        if (this.RecentScoresCount.Value > 0)
        {
            _ = uriBuilder.QueryParam("count", this.RecentScoresCount.Value);
        }

        var uri = uriBuilder.ToUri().PathAndQuery;

        var newScores = await this.HttpClient.GetFromJsonAsync<PlayerScore[]>(uri, cancellationToken);
        if (newScores?.Length > 0)
        {
            var scoresToAdd = newScores
                .Select(x => x with
                {
                    LeaderBoard = x.LeaderBoard with
                    {
                        CoverImage = $@"/api/images/{Uri.EscapeDataString(x.LeaderBoard.CoverImage)}",
                    },
                })
                .OrderBy(x => x.Score.TimeSet);

            foreach (var item in scoresToAdd)
            {
                this.RecentScores.Insert(0, item);
            }
        }
    }

    private async Task UpdatePlayerAsync(CancellationToken cancellationToken = default)
    {
        var player = await this.HttpClient
            .GetFromJsonAsync<Player>(
                $"/api/players/{this.Snapshot.PlayerId}/platforms/{this.Snapshot.PlatformId}/profile",
                cancellationToken);

        if (player is not null)
        {
            this.Player.Value = player with
            {
                ProfilePicture = $@"/api/images/{Uri.EscapeDataString(player.ProfilePicture)}",
            };
        }
    }

    #region IDisposable

    private bool isDisposed = false;
    public void Dispose()
    {
        this.Dispose(true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        if (this.isDisposed)
        { return; }

        if (disposing)
        {
            this.Disposables.Dispose();
        }

        this.isDisposed = true;
    }

    private void ThrowIfDisposed()
    {
        if (this.isDisposed)
        {
            throw new ObjectDisposedException(this.ToString());
        }
    }

    #endregion IDisposable
}
