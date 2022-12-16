using System.Reactive.Disposables;

namespace RankOverlay.Web.BlazorWasm.Client.Pages;

public sealed class Session : IDisposable
{
    private ICancelable? currentSession = null;
    public Session()
    {
    }

    public Session(ICancelable session)
    {
        Volatile.Write(ref this.currentSession, session);
    }

    public void StartNew(ICancelable session)
    {
        this.ThrowIfDisposed();

        Interlocked
            .Exchange(ref this.currentSession, session)
            ?.Dispose();
    }

    public void Stop()
    {
        Interlocked
            .Exchange(ref this.currentSession, null)
            ?.Dispose();
    }

    public void StartNew(Action<ICollection<IDisposable>> sessionFactory)
    {
        var disposables = new List<IDisposable>();
        sessionFactory.Invoke(disposables);

        var session = StableCompositeDisposable.Create(disposables.ToArray());
        this.StartNew(session);
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
            Interlocked
                .Exchange(ref this.currentSession, null)
                ?.Dispose();
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
