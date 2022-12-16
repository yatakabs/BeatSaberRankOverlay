using System.Runtime.CompilerServices;
using Microsoft.Extensions.Options;
using RankOverlay.PlayerScores;
using RankOverlay.RankProviders.ScoreSaber;
using RankOverlay.Scores.Snapshots;
using Storage.Net.Blobs;

namespace RankOverlay.Web.BlazorWasm.Server.BlobStorage;

public class ScoreSaberBlobStoragePlayerScoreSnapshotRepository : IPlayerScoreSnapshotRepository
{
    private IBlobStorage BlobStorage { get; }
    private BlobStorageOptions Options { get; }

    public RankPlatformId PlatformId { get; }
    public PlayerId PlayerId { get; }

    public ScoreSaberBlobStoragePlayerScoreSnapshotRepository(
        PlayerId playerId,
        RankPlatformId scoreSource,
        IBlobStorage blobStorage,
        IOptionsSnapshot<BlobStorageOptions> options)
    {
        this.PlayerId = playerId;
        this.BlobStorage = blobStorage;
        this.PlatformId = scoreSource;
        this.Options = options.Value;
    }

    #region Helper methods
    private string GetBaseFolderPath()
    {
        return Path.Combine(
            "players",
            this.PlayerId.ToString(),
            "snapshots",
            (string)this.PlatformId);
    }

    private string GetSnapshotFolderPath(
        PlayerScoreSnapshotId snapshotId)
    {
        var time = ((Ulid)snapshotId).Time.ToOffset(this.Options.TimeZoneOffset ?? TimeSpan.Zero);
        return this.GetSnapshotFolderPath(time);
    }

    private string GetSnapshotFolderPath(
        DateTimeOffset time)
    {
        var localTime = time
            .ToOffset(this.Options.TimeZoneOffset ?? TimeSpan.Zero);

        return Path.Combine(
            this.GetBaseFolderPath(),
            localTime.Year.ToString("D4"),
            localTime.Month.ToString("D2"),
            localTime.Day.ToString("D2"));
    }

    public async Task<DateTime> GetOldestSnapshotDateAsync(
        CancellationToken cancellationToken)
    {
        var minYear = await this
            .ListFoldersAsync(
                recursive: false,
                cancellationToken: cancellationToken)
            .Select(x => int.TryParse(x.Name, out var year) ? year : 0)
            .Where(x => x > 0)
            .OrderBy(x => x)
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);

        var minMonth = minYear == 0
            ? 1
            : await this
                .ListFoldersAsync(
                    year: minYear,
                    recursive: false,
                    cancellationToken: cancellationToken)
                .Select(x => int.TryParse(x.Name, out var month) ? month : 0)
                .Where(x => x > 0)
                .OrderBy(x => x)
                .FirstOrDefaultAsync(cancellationToken)
                .ConfigureAwait(false);

        var minDay = minYear == 0 || minMonth == 0
            ? 1
            : await this.ListFoldersAsync(
                year: minYear,
                month: minMonth,
                recursive: false,
                cancellationToken: cancellationToken)
            .Select(x => int.TryParse(x.Name, out var day) ? day : 0)
            .Where(x => x > 0)
            .OrderBy(x => x)
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);

        return minYear == 0
            ? DateTime.MinValue
            : new DateTime(
                year: minYear,
                month: minMonth,
                day: minDay);
    }

    public async Task<DateTime> GetLatestSnapshotDateAsync(
        CancellationToken cancellationToken)
    {
        var maxYear = await this
            .ListFoldersAsync(
                recursive: false,
                cancellationToken: cancellationToken)
            .Select(x => int.TryParse(x.Name, out var year) ? year : 0)
            .Where(x => x > 0)
            .OrderByDescending(x => x)
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);

        var maxMonth = maxYear == 0
            ? 0
            : await this
                .ListFoldersAsync(
                    year: maxYear,
                    recursive: false,
                    cancellationToken: cancellationToken)
                .Select(x => int.TryParse(x.Name, out var month) ? month : 0)
                .Where(x => x > 0)
                .OrderByDescending(x => x)
                .FirstOrDefaultAsync(cancellationToken)
                .ConfigureAwait(false);

        var maxDay = maxYear == 0 || maxMonth == 0
            ? 0
            : await this.ListFoldersAsync(
                year: maxYear,
                month: maxMonth,
                recursive: false,
                cancellationToken: cancellationToken)
            .Select(x => int.TryParse(x.Name, out var day) ? day : 0)
            .Where(x => x > 0)
            .OrderByDescending(x => x)
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);

        return maxYear == 0
            ? DateTime.MaxValue
            : new DateTime(
                year: maxYear,
                month: maxMonth,
                day: maxDay);
    }

    private static IEnumerable<string> GetFragments(
        int? year = null,
        int? month = null,
        int? day = null)
    {
        if (year is not null)
        {
            yield return year.Value.ToString("D4");
        }

        if (month is not null)
        {
            yield return year is null
                ? throw new ArgumentException(nameof(year), $"Value must be set when {nameof(month)} is specified.")
                : month is < 1 or > 12
                ? throw new ArgumentOutOfRangeException(nameof(month), "Value must be an integer between 1 and 12.")
                : month.Value.ToString("D2");
        }

        if (day is not null)
        {
            if (year is null)
            {
                throw new ArgumentException(nameof(year), $"Value must be set when {nameof(day)} is specified.");
            }
            else if (month is null)
            {
                throw new ArgumentException(nameof(month), $"Value must be set when {nameof(day)} is specified.");
            }
            else if (day < 1 || day > DateTime.DaysInMonth(year.Value, month.Value))
            {
                throw new ArgumentOutOfRangeException(nameof(day), $"Invalid {nameof(day)} value for the specified pair of {nameof(year)} and {nameof(month)}");
            }

            yield return day.Value.ToString("D2");
        }
    }

    private string GetFolderPath(int year)
    {
        return Path.Combine(
            this.GetBaseFolderPath(),
            year.ToString("D4"));
    }

    private string GetFolderPath(
        int year,
        int month)
    {
        return Path.Combine(
            this.GetFolderPath(year),
            month.ToString("D2"));
    }

    private string GetFolderPath(
        int year,
        int month,
        int day)
    {
        return Path.Combine(
            this.GetFolderPath(year, month),
            day.ToString("D2"));
    }

    private IAsyncEnumerable<Blob> ListFoldersAsync(
        int? year = null,
        int? month = null,
        int? day = null,
        bool recursive = true,
        CancellationToken cancellationToken = default)
    {
        var fragments = new List<string>
        {
            this.GetBaseFolderPath()
        };
        fragments.AddRange(GetFragments(year, month, day));

        var scanRootFolderPath = Path.Combine(fragments.ToArray());

        return this.BlobStorage
            .ListAsync(
                new ListOptions
                {
                    FolderPath = scanRootFolderPath,
                    Recurse = recursive,
                    BrowseFilter = x => x.IsFolder,
                },
                cancellationToken)
            .ToAsyncEnumerable()
            .SelectMany(x => x.ToAsyncEnumerable());
    }

    private async IAsyncEnumerable<Blob> ListFilesRecursively(
        string rootFolderPath,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var blobs = await this.BlobStorage
            .ListAsync(
                new ListOptions
                {
                    FolderPath = rootFolderPath,
                    Recurse = false,
                },
                cancellationToken)
            .ConfigureAwait(false);

        foreach (var blob in blobs)
        {
            if (blob.IsFile)
            {
                yield return blob;
            }
            else if (blob.IsFolder)
            {
                var fileBlobs = this.ListFilesRecursively(
                    blob.FullPath,
                    cancellationToken)
                    .WithCancellation(cancellationToken);

                await foreach (var file in fileBlobs)
                {
                    yield return file;
                }
            }
            else
            {
                throw new NotSupportedException("Unsupported blob type.");
            }
        }
    }

    private string GetSnapshotFilePath(
        PlayerScoreSnapshotId snapshotId)
    {
        var directory = this.GetSnapshotFolderPath(snapshotId);

        return Path.Combine(
            directory,
            $"snapshot_{snapshotId.ToString().ToLower()}.json");
    }
    #endregion

    #region IPlayerScoreSnapshotRepository

    public async Task<PlayerScoreSnapshot?> GetSnapshotOrDefaultAsync(
        PlayerScoreSnapshotId snapshotId,
        Func<PlayerScoreSnapshotId, PlayerScoreSnapshot?> defaultValueProvider,
        CancellationToken cancellationToken = default)
    {
        var snapshot = await this.LoadSnapshotOrDefaultAsync(
            snapshotId,
            cancellationToken);

        return snapshot
            ?? defaultValueProvider?.Invoke(snapshotId);
    }

    public async Task DeleteSnapshotAsync(
        PlayerScoreSnapshotId snapshotId,
        CancellationToken cancellationToken = default)
    {
        var snapshotFilePath = this.GetSnapshotFilePath(snapshotId);

        await this.BlobStorage.DeleteAsync(
            fullPath: snapshotFilePath,
            cancellationToken: cancellationToken);
    }

    public async IAsyncEnumerable<PlayerScoreSnapshot> ListSnapshotAsync(
        SnapshotListingOptions options,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var fromDate = options.From.HasValue
            ? options.From.Value.ToOffset(this.Options.TimeZoneOffset ?? TimeSpan.Zero).Date
            : await this
                .GetOldestSnapshotDateAsync(cancellationToken)
                .ConfigureAwait(false);

        var toDate = options.To.HasValue
            ? options.To.Value.ToOffset(this.Options.TimeZoneOffset ?? TimeSpan.Zero).Date
            : await this
                .GetLatestSnapshotDateAsync(cancellationToken)
                .ConfigureAwait(false);

        if (fromDate == DateTime.MinValue || toDate == DateTime.MaxValue)
        {
            // No snapshot folder/file available
            yield break;
        }

        var dayRange = (int)Math.Floor((toDate - fromDate).TotalDays) + 1;
        var snapshots = Enumerable
            .Range(0, dayRange)
            .Select(i => fromDate.AddDays(i))
            .ToAsyncEnumerable()
            .SelectManyAwaitWithCancellation((date, ct) =>
                ValueTask.FromResult(this.BlobStorage
                    .ListFilesAsync(
                        new ListOptions
                        {
                            FolderPath = this.GetFolderPath(
                                date.Year,
                                date.Month,
                                date.Day),
                            Recurse = false,
                            FilePrefix = "snapshot_",
                        },
                        ct)
                    .ToAsyncEnumerable()
                    .SelectMany(x => x.ToAsyncEnumerable())))
            .SelectAwaitWithCancellation(async (blob, ct) =>
                await this.BlobStorage
                    .ReadJsonAsync<ScoreSaberPlayerSnapshot>(
                        fullPath: blob.FullPath,
                        ignoreInvalidJson: true,
                        options: this.Options.JsonSerializerOptions,
                        encoding: this.Options.TextEncoding,
                        cancellationToken: ct)
                    .ConfigureAwait(false))
            .Where(x => x is not null);

        if (options.From.HasValue)
        {
            snapshots = snapshots.Where(x => options.From.Value <= x.Timestamp);
        }

        if (options.To.HasValue)
        {
            snapshots = snapshots.Where(x => x.Timestamp < options.To.Value);
        }

        if (options.SkipCount > 0)
        {
            snapshots = snapshots.Skip(options.SkipCount.Value);
        }

        if (options.TakeCount > 0)
        {
            snapshots = snapshots.Take(options.TakeCount.Value);
        }

        await foreach (var snapshot in snapshots)
        {
            yield return snapshot;
        }
    }

    public async Task<PlayerScoreSnapshot> UpdateSnapshotAsync(
        PlayerScoreSnapshotId snapshotId,
        Func<PlayerScoreSnapshot, CancellationToken, ValueTask<PlayerScoreSnapshot>> update,
        CancellationToken cancellationToken)
    {
        var snapshot = await this.LoadSnapshotOrDefaultAsync(
            snapshotId,
            cancellationToken);

        if (snapshot is null)
        {
            throw new KeyNotFoundException("Snapshot not found.");
        }

        var newSnapshot = await update.Invoke(
            snapshot,
            cancellationToken);

        if (newSnapshot.Id != snapshot.Id)
        {
            throw new InvalidOperationException("Snapshot ID cannot be changed.");
        }

        if (newSnapshot.PlatformId != this.PlatformId)
        {
            throw new InvalidOperationException("Rank Platform ID cannot be changed.");
        }

        if (newSnapshot is not ScoreSaberPlayerSnapshot scoreSaberPlayerSnapshot)
        {
            throw new InvalidOperationException("Snapshot type could not be changed.");
        }

        await this.SaveSnapshotAsync(
            scoreSaberPlayerSnapshot,
            cancellationToken);

        return scoreSaberPlayerSnapshot;
    }

    public async Task<PlayerScoreSnapshot> CreateSnapshotAsync(
        Func<PlayerScoreSnapshotId, CancellationToken, ValueTask<PlayerScoreSnapshot>> create,
        CancellationToken cancellationToken)
    {
        var snapshotId = PlayerScoreSnapshotId.New();
        var snapshot = await create.Invoke(snapshotId, cancellationToken);

        if (snapshot.Id != snapshotId)
        {
            throw new InvalidOperationException("Given snapshot ID must be set.");
        }

        if (snapshot is not ScoreSaberPlayerSnapshot scoreSaberPlayerSnapshot)
        {
            throw new InvalidOperationException($"Snapshot type must be {nameof(ScoreSaberPlayerSnapshot)}.");
        }

        await this.SaveSnapshotAsync(
            scoreSaberPlayerSnapshot,
            cancellationToken);

        return scoreSaberPlayerSnapshot;
    }

    private async Task<ScoreSaberPlayerSnapshot> LoadSnapshotOrDefaultAsync(
        PlayerScoreSnapshotId snapshotId,
        CancellationToken cancellationToken)
    {
        var snapshotFilePath = this.GetSnapshotFilePath(snapshotId);

        return await this.BlobStorage
            .ReadJsonAsync<ScoreSaberPlayerSnapshot>(
                fullPath: snapshotFilePath,
                ignoreInvalidJson: true,
                options: this.Options.JsonSerializerOptions,
                encoding: this.Options.TextEncoding,
                cancellationToken: cancellationToken)
            .ConfigureAwait(false);
    }

    private async Task SaveSnapshotAsync(
        ScoreSaberPlayerSnapshot snapshot,
        CancellationToken cancellationToken)
    {
        var snapshotFilePath = this.GetSnapshotFilePath(snapshot.Id);

        await this.BlobStorage
            .WriteJsonAsync(
                fullPath: snapshotFilePath,
                instance: snapshot,
                options: this.Options.JsonSerializerOptions,
                encoding: this.Options.TextEncoding,
                cancellationToken: cancellationToken);
    }
    #endregion
}

