using System.ComponentModel;

namespace WPFTemplate.App.Command;

/// <summary>
/// Represents a visible command object that can carry an active/selected indicator.
/// </summary>
internal class CommandViewModel : RelayCommand, INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    internal CommandViewModel(string header, Action<object?> execute, Func<object?, bool>? canExecute = null)
        : base(execute, canExecute) => Header = header;

    internal CommandViewModel(string header, object icon, Action<object?> execute, Func<object?, bool>? canExecute = null)
        : base(execute, canExecute) => (Header, Icon) = (header, icon);

    /// <summary>The text to display.</summary>
    public string Header { get; }

    /// <summary>The icon to display.</summary>
    public object? Icon { get; }

    /// <summary>
    /// True when this command represents the currently active selection.
    /// Drives <c>MenuItem.IsChecked</c> in the menu.
    /// </summary>
    public bool IsActive
    {
        get => field;
        set
        {
            if (field == value)
                return;
            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsActive)));
        }
    }
}
