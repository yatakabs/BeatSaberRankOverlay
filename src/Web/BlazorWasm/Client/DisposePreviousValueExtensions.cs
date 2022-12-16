using System.Reactive.Disposables;
using System.Reactive.Linq;

public static class DisposePreviousValueExtensions
{
    /// <summary>
    /// Disposes previous value automatically.
    /// </summary>
    public static IObservable<T> DisposePreviousValue<T>(this IObservable<T> source)
    {
        return Observable.Create<T>(ox =>
        {
            var d = new SerialDisposable();
            return new CompositeDisposable(
                source.Do(x => d.Disposable = x as IDisposable).Do(ox).Subscribe(),
                d);
        });
    }
}
