using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace RankOverlay.Web.BlazorWasm.Client.Components;

public class AutoBindingView<T> : OwningComponentBase
{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    [Parameter]
    public RenderFragment<T> ChildContent { get; set; }

    [Parameter]
    public EventCallback<T> ContextCreated { get; set; }

    [Parameter]
    public object[] Parameters { get; set; }

    [Inject]
    protected NavigationManager NavigationManager { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

    public T? DataContext { get; private set; }

    protected override void OnInitialized()
    {
        this.DataContext = this.Parameters is null
            ? ActivatorUtilities.CreateInstance<T>(this.ScopedServices)
            : ActivatorUtilities.CreateInstance<T>(this.ScopedServices, this.Parameters);

        if (this.DataContext is INotifyStateChanged notifyStateChanged)
        {
            notifyStateChanged.StateChanged += this.NotifyStateChanged_StateChanged;
        }

        if (this.NavigationManager != null && this.DataContext is IRequireNavigationManager requireNavigationManager)
        {
            requireNavigationManager.SetNavigateionManager(this.NavigationManager);
        }
    }

    protected override Task OnAfterRenderAsync(bool firstRender)
    {
        return firstRender ? this.ContextCreated.InvokeAsync(this.DataContext) : Task.CompletedTask;
    }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        if (this.ChildContent != null)
        {
            builder.AddContent(0, this.ChildContent.Invoke(this.DataContext!));
        }
    }

    private void NotifyStateChanged_StateChanged(object? sender, EventArgs e)
    {
        this.StateHasChanged();
    }

    #region IDisposable

    private bool isDisposed = false;

    protected override void Dispose(bool disposing)
    {
        if (!this.isDisposed)
        {
            if (disposing)
            {
                if (this.DataContext is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }

            this.isDisposed = true;
        }

        base.Dispose(disposing);
    }

    #endregion IDisposable
}
