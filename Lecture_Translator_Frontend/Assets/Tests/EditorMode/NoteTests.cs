using System;
using NUnit.Framework;
using UnityEditor.VersionControl;
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

}
