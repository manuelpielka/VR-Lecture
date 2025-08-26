using System;
using NUnit.Framework;
using UnityEngine;

/// <summary>
/// Contains unit tests for the <see cref="Note"/> class.
/// Verifies correct behavior when creating notes with valid and invalid titles.
/// </summary>
public class NoteTests
{
    /// <summary>
    /// Verifies that creating a note with a valid title and content
    /// correctly sets the Title and Content properties.
    /// </summary>
    [Test]
    public void CreateNote_WithValidTitle()
    {
        var note = new Note("Test_New-Note", "Create new note with valid title.");
        Assert.AreEqual("Test_New-Note", note.Title);
        Assert.AreEqual("Create new note with valid title.", note.Content);
    }

    /// <summary>
    /// Verifies that creating a note with an empty or whitespace-only title
    /// throws an <see cref="ArgumentException"/>.
    /// </summary>
    [Test]
    public void CreateNote_WithEmptyTitle()
    {
        Assert.Throws<ArgumentException>(() =>
        {
            var note = new Note("", "Content");
        });

        Assert.Throws<ArgumentException>(() =>
        {
            var note = new Note("   ", "Content");
        });
    }

    /// <summary>
    /// Verifies that creating a note with invalid characters in the title
    /// throws an <see cref="ArgumentException"/>.
    /// </summary>
    /// <param name="invalidTitle">A title string containing invalid characters</param>
    [TestCase("TitleWith Space")]
    [TestCase("TitleWith/Slash")]
    [TestCase("TitleWith\\Backslash")]
    [TestCase("TitleWIth@Symbol")]
    [TestCase("TitleWith.Dot")]
    public void CreateNote_WithInvalidTitle_Throws(string invalidTitle)
    {
        Assert.Throws<ArgumentException>(() =>
        {
            new Note(invalidTitle, "Content");
        });
    }

    /// <summary>
    /// Verifies the parameterless constructor exists for Unity deserialization
    /// and keeps default values.
    /// </summary>
    [Test]
    public void DefaultCtor_ForUnityDeserialization_LeavesDefaults()
    {
        var n = new Note(); // cover the empty .ctor
        Assert.IsNull(n.Title);
        Assert.IsNull(n.LectureTitle);
        Assert.AreEqual(0d, n.CreatedAtSeconds); // default (or explicit 0d if you applied step A)
    }

    /// <summary>
    /// CreatedAtSeconds participates in (de)serialization so the UI can use it.
    /// Round-trip through Unity's JsonUtility must preserve the value.
    /// </summary>
    [Test]
    public void Serialization_RoundTrip_Preserves_CreatedAtSeconds()
    {
        var ts = 1724679900d; // pretend capture time
        var note = new Note("1724679900", "Lecture body")
        {
            LectureTitle = "My Lecture",
            CreatedAtSeconds = ts
        };

        var json = JsonUtility.ToJson(note);
        var round = JsonUtility.FromJson<Note>(json);

        Assert.AreEqual(ts, round.CreatedAtSeconds);
        Assert.AreEqual("1724679900", round.Title); // lecture notes use timestamp as title
        Assert.AreEqual("My Lecture", round.LectureTitle);
    }


}
