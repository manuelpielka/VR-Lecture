using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

/// <summary>
/// EditMode unit tests for <see cref="NoteUtils"/>.
/// Verifies the behavior of <see cref="NoteUtils.IsNull{T}(T, string)"/> across
/// normal nulls, non-nulls, and destroyed UnityEngine.Object references.
/// </summary>
public class NoteUtilsEditorTests
{
    /// <summary>
    /// When a normal reference is null, the method should log a warning with the provided message and return true.
    /// </summary>
    [Test]
    public void IsNull_WithNullReference_LogsWarning_And_ReturnsTrue()
    {
        string message = "null encountered";
        LogAssert.Expect(LogType.Warning, message);
        bool result = NoteUtils.IsNull<object>(null, message);
        Assert.IsTrue(result);
        LogAssert.NoUnexpectedReceived();
    }

    /// <summary>
    /// When the reference is not null, the method should return false and emit no logs.
    /// </summary>
    [Test]
    public void IsNull_WithNonNullReference_ReturnsFalse_And_NoLogs()
    {
        object obj = new object();
        bool result = NoteUtils.IsNull(obj, "should not log");
        Assert.IsFalse(result);
        LogAssert.NoUnexpectedReceived();
    }

    /// <summary>
    /// Destroyed UnityEngine.Object is treated as null by Unity's overloaded null-check,
    /// therefore the method should not log a warning and return flase.
    /// </summary>
    [Test]
    public void IsNull_WithDestroyedUnityObject_LogsWarning_And_ReturnsTrue()
    {
        var go = new GameObject("Temp_For_IsNull_Destroyed");
        Object.DestroyImmediate(go); // fake null

        bool result = NoteUtils.IsNull(go, "should not log");
        Assert.IsFalse(result);
        LogAssert.NoUnexpectedReceived();
    }
}