using System;
using System.Windows.Input;

namespace steam.Utils;

public class RelayCommand : ICommand {
    private readonly Action execute;
    private readonly Func<bool>? canExecute;

    public RelayCommand(Action execute, Func<bool>? canExecute = null) {
        this.execute = execute ?? throw new ArgumentNullException(nameof(execute));
        this.canExecute = canExecute;
    }

    public event EventHandler? CanExecuteChanged;

    public bool CanExecute(object? parameter) => this.canExecute?.Invoke() ?? true;

    public void Execute(object? parameter) => this.execute();

    public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
}

public class RelayCommand<T> : ICommand {
    private readonly Action<T?> execute;
    private readonly Func<T?, bool>? canExecute;

    public RelayCommand(Action<T?> execute, Func<T?, bool>? canExecute = null) {
        this.execute = execute ?? throw new ArgumentNullException(nameof(execute));
        this.canExecute = canExecute;
    }

    public event EventHandler? CanExecuteChanged;

    public bool CanExecute(object? parameter) => this.canExecute?.Invoke((T?)parameter) ?? true;

    public void Execute(object? parameter) => this.execute((T?)parameter);

    public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
}