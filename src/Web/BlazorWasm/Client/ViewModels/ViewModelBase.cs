using LazyProperty;
using RankOverlay.Web.BlazorWasm.Client.Components;

namespace RankOverlay.Web.BlazorWasm.Client.ViewModels;

public class ViewModelBase : LazyPropertyHolderBase, INotifyStateChanged
{
    public ViewModelBase()
    {
        this.PropertyChanged += this.ViewModelBase_PropertyChanged;
    }

    private void ViewModelBase_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        this.NotifyStateChanged();
    }

    protected void NotifyStateChanged()
    {
        this.StateChanged?.Invoke(this, EventArgs.Empty);
    }

    public event EventHandler? StateChanged;
}

