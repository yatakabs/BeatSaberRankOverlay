using System.Reactive.Disposables;
using Microsoft.AspNetCore.Components;
using RankOverlay.Web.BlazorWasm.Client.Components;

namespace RankOverlay.Web.BlazorWasm.Client.ViewModels.Pages;

public class PageViewModelBase : ReactiveViewModelBase, IRequireNavigationManager
{
    protected CompositeDisposable Disposables { get; } = new CompositeDisposable();

    protected NavigationManager? NavigationManager { get; private set; }

    public void SetNavigateionManager(NavigationManager navigationManager)
    {
        this.NavigationManager = navigationManager;
    }

    #region IDisposable

    private bool isDisposed = false;

    protected override void Dispose(bool disposing)
    {
        if (!this.isDisposed)
        {
            if (disposing)
            {
                this.Disposables.Dispose();
            }

            this.isDisposed = true;
        }

        base.Dispose(disposing);
    }

    #endregion IDisposable
}
