using System;
using NUnit.Framework;
using UnityEditor.VersionControl;
using UnityEngine;


public class NoteTests
{
    [Test]
    public void CreateNote_WithValidTitle()
    {
        var note = new Note("Test_New-Note", "Create new note with valid title.");
        Assert.AreEqual("Test_New-Note", note.Title);
        Assert.AreEqual("Create new note with valid title.", note.Content);
    }

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

    [Test]
    public void CreatetNote_WithInvalidTitle()
    {
        string[] invalidTitles = {
            "TitleWith Space",
            "TitleWith/Slash",
            "TitleWith\\Backslash",
            "TitleWIth@Symbol",
            "TitleWith.Dot"
        };

        foreach (var title in invalidTitles)
        {
            Assert.Throws<ArgumentException>(() =>
            {
                var note = new Note(title, "Content");
            }, $"Title '{title}' is invalid.");
        }
    }

}
