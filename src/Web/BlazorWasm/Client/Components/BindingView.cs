using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace RankOverlay.Web.BlazorWasm.Client.Components;

public class BindingView<T> : ComponentBase, IDisposable
{
    private T? dataContext;

    [Parameter]
    public T? DataContext
    {
        get => this.dataContext;
        set
        {
            if (this.dataContext is INotifyStateChanged oldNotifyStateChanged)
            {
                oldNotifyStateChanged.StateChanged -= this.NotifyStateChanged_StateChanged;
            }

            this.dataContext = value;

            if (this.dataContext is INotifyStateChanged newNotifyStateChanged)
            {
                newNotifyStateChanged.StateChanged += this.NotifyStateChanged_StateChanged;
            }
        }
    }

    [Parameter]
    public RenderFragment<T?>? ChildContent { get; set; }

    [Parameter]
    public RenderFragment<T>? NotNullContent { get; set; }

    [Parameter]
    public RenderFragment? NullContent { get; set; }

    [Parameter]
    public bool DisposeDataContext { get; set; } = true;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    [Inject]
    protected NavigationManager NavigationManager { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        var dataContext = this.DataContext;
        if (dataContext is not null && this.NotNullContent is not null)
        {
            builder.AddContent(0, this.NotNullContent.Invoke(dataContext));
        }
        else if (dataContext is null && this.NullContent is not null)
        {
            builder.AddContent(0, this.NullContent);
        }
        else if (this.ChildContent is not null)
        {
            builder.AddContent(0, this.ChildContent.Invoke(dataContext));
        }
    }

    protected override void OnParametersSet()
    {
        if (this.NavigationManager != null && this.DataContext is IRequireNavigationManager requireNavigationManager)
        {
            requireNavigationManager.SetNavigateionManager(this.NavigationManager);
        }
    }

    private void NotifyStateChanged_StateChanged(object? sender, EventArgs e)
    {
        this.StateHasChanged();
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
            if (this.DisposeDataContext && this.DataContext is IDisposable disposable)
            {
                disposable.Dispose();
            }
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
