namespace InnoJob.App.Command;

/// <summary>
/// Represents a visible command object.
/// </summary>
internal class CommandViewModel : RelayCommand
{
    internal CommandViewModel(string header, Action<object?> execute, Func<object?, bool>? canExecute = null)
        : base(execute, canExecute) => Header = header;

    internal CommandViewModel(string header, object icon, Action<object?> execute, Func<object?, bool>? canExecute = null)
        : base(execute, canExecute) => (Header, Icon) = (header, icon);

    /// <summary>
    /// The text to display.
    /// </summary>
    public string Header { get; }

    /// <summary>
    /// The icon to display.
    /// </summary>
    public object? Icon { get; }
}
