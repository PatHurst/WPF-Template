using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace WPFTemplate.App.ViewModels;

/// <summary>
/// Base class for objects that supply UI data.
/// </summary>
internal abstract class ObservableObject : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Set the backing field and raise property changed.
    /// </summary>
    /// <typeparam name="T">The property type.</typeparam>
    /// <param name="field">The reference to the backing field.</param>
    /// <param name="value">The new value.</param>
    /// <param name="property">The property name. Automatically filled by <see cref="CallerMemberNameAttribute"/></param>
    protected void SetProperty<T>(ref T field, T value, [CallerMemberName] string property = "")
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
            return;

        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
    }

    /// <summary>
    /// Raise property changed.
    /// </summary>
    /// <param name="property">The property name. Automatically filled by <see cref="CallerMemberNameAttribute"/></param>
    protected void RaisePropertyChanged([CallerMemberName] string property = "") =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));

}
