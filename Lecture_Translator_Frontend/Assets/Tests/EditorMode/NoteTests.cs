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
