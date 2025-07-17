using UnityEngine;

public class NoteUtils
{
    /// <summary>
    /// Thsi method checks if the provided object is null and logs a warning with a custom message.
    /// </summary>
    /// <typeparam name="T">The type of object being checked.</typeparam>
    /// <param name="obj">The object to check.</param>
    /// <param name="warningMessage">The warning message to log if the object is null.</param>
    /// <returns>True if the object is null; Otherwise false.</returns>
    public static bool IsNull<T>(T obj, string warningMessage)
    {
        if (obj == null)
        {
            Debug.LogWarning(warningMessage);
            return true;
        }
        return false;
    }
}
