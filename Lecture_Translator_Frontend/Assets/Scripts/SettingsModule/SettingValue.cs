using UnityEngine;

/// <summary>
/// A class that stores a single setting's value.
/// </summary>
public class SettingValue
{
    // Private field to store the actual value of the setting.
    private object value;

    /// <summary>
    /// Constructor to initialize the setting with a value.
    /// </summary>
    /// <param name="value">The initial value to assign to the setting.</param>
    public SettingValue(object value)
    {
        this.value = value;
    }

    /// <summary>
    /// Gets the current value of the setting.
    /// </summary>
    /// <returns>The stored value as an object.</returns>
    public object GetValue()
    {
        return value;
    }

    /// <summary>
    /// Updates the setting with a new value.
    /// </summary>
    /// <param name="value">The new value to assign.</param>
    public void SetValue(object value)
    {
        this.value = value;
    }

    /// <summary>
    /// Returns a string representation of the stored value.
    /// If the value is null, returns the string "null".
    /// </summary>
    /// <returns>A string version of the value or "null".</returns>
    public override string ToString()
    {
        return value != null ? value.ToString() : "null";
    }
}
