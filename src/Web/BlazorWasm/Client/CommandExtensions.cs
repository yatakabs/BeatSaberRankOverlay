using System.Windows.Input;

namespace RankOverlay.Web.BlazorWasm.Client;

public static class CommandExtensions
{
    public static Action AsAction<TCommand>(
        this TCommand command,
        object parameter)
        where TCommand : ICommand
    {
        return () =>
        {
            if (command?.CanExecute(parameter) == true)
            {
                command.Execute(parameter);
            }
        };
    }

    public static Action AsAction<TCommand>(
        this TCommand command,
        Func<object> parameterProvider)
        where TCommand : ICommand
    {
        return () =>
        {
            if (command?.CanExecute(parameterProvider.Invoke()) == true)
            {
                command.Execute(parameterProvider.Invoke());
            }
        };
    }

    public static Action AsAction<TCommand>(this TCommand command)
        where TCommand : ICommand
    {
        return () =>
        {
            if (command?.CanExecute(null) == true)
            {
                command.Execute(null);
            }
        };
    }
    public static Func<bool> CanExecuteAsAction<TCommand>(
    this TCommand command,
    object parameter)
    where TCommand : ICommand
    {
        return () => command?.CanExecute(parameter) == true;
    }

    public static Func<bool> CanExecuteAsAction<TCommand>(
        this TCommand command,
        Func<object> parameterProvider)
        where TCommand : ICommand
    {
        return () => command?.CanExecute(parameterProvider.Invoke()) == true;
    }

    public static Func<bool> CanExecuteAsAction<TCommand>(this TCommand command)
        where TCommand : ICommand
    {
        return () => command?.CanExecute(null) == true;
    }

    public static Func<bool> DisabledAsAction<TCommand>(
        this TCommand command,
        object parameter)
    where TCommand : ICommand
    {
        return () => !(command?.CanExecute(parameter) == true);
    }

    public static Func<bool> DisabledAsAction<TCommand>(
        this TCommand command,
        Func<object> parameterProvider)
        where TCommand : ICommand
    {
        return () => !(command?.CanExecute(parameterProvider.Invoke()) == true);
    }

    public static Func<bool> DisabledAsAction<TCommand>(this TCommand command)
        where TCommand : ICommand
    {
        return () => !(command?.CanExecute(null) == true);
    }

    public static bool IsDisabled<TCommand>(
        this TCommand command,
        object parameter)
        where TCommand : ICommand
    {
        return !(command?.CanExecute(parameter) == true);
    }

    public static bool IsDisabled<TCommand>(
        this TCommand command,
        Func<object> parameterProvider)
        where TCommand : ICommand
    {
        return !(command?.CanExecute(parameterProvider.Invoke()) == true);
    }

    public static bool IsDisabled<TCommand>(this TCommand command)
        where TCommand : ICommand
    {
        return !(command?.CanExecute(null) == true);
    }
}
