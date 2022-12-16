using System.ComponentModel;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Windows.Input;
using LazyProperty;
using Reactive.Bindings.Extensions;

namespace RankOverlay.Web.BlazorWasm.Client.ViewModels;

public class ReactiveViewModelBase : ViewModelBase, IReactiveLazyPropertyHolder
{
    private CompositeDisposable Disposables { get; } = new CompositeDisposable();

    ICollection<IDisposable> IReactiveLazyPropertyHolder.Disposables => this.Disposables;

    private Dictionary<string, INotifyPropertyChanged> NotificationObjectMap { get; } = new Dictionary<string, INotifyPropertyChanged>();
    private Dictionary<string, ICommand> CommandsMap { get; } = new Dictionary<string, ICommand>();
    private Subject<Unit> PropertyValueChangedSubject { get; }

    public ReactiveViewModelBase()
    {
        this.PropertyChanged += this.ReactiveViewModel_PropertyChanged;

        this.PropertyValueChangedSubject = new Subject<Unit>()
            .AddTo(this.Disposables);

        this.PropertyValueChangedSubject
            .ObserveOn(CurrentThreadScheduler.Instance)
            .Subscribe(_ => this.NotifyStateChanged())
            .AddTo(this.Disposables);
    }

    private void ReactiveViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is null)
        {
            return;
        }

        if (this.NotificationObjectMap.TryGetValue(e.PropertyName, out var oldValue) && oldValue != null)
        {
            oldValue.PropertyChanged -= this.NotifyPropertyChanged_PropertyChanged;
        }

        if (this.CommandsMap.TryGetValue(e.PropertyName, out var oldCommand))
        {
            oldCommand.CanExecuteChanged -= this.Command_CanExecuteChanged;
        }

        if (this.TryGetValueAsObject(e.PropertyName, out var value) && value is INotifyPropertyChanged notifyPropertyChanged)
        {
            notifyPropertyChanged.PropertyChanged += this.NotifyPropertyChanged_PropertyChanged;
        }

        if (this.CommandsMap.TryGetValue(e.PropertyName, out var command))
        {
            command.CanExecuteChanged += this.Command_CanExecuteChanged;
        }
    }

    private void Command_CanExecuteChanged(object? sender, EventArgs e)
    {
        this.PropertyValueChangedSubject.OnNext(Unit.Default);
    }

    private void NotifyPropertyChanged_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        this.PropertyValueChangedSubject.OnNext(Unit.Default);
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

            foreach (var value in this.NotificationObjectMap.Values.ToArray())
            {
                value.PropertyChanged -= this.NotifyPropertyChanged_PropertyChanged;
            }

            this.NotificationObjectMap.Clear();
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
