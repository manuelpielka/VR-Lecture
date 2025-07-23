using System.IO;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

public class NoteManagerTests
{
    private NoteManager manager;

    // Used to verify if notes are correctly saved or deleted during tests.
    private string notesPath;

    [SetUp]
    public void SetUp()
    {
        manager = new NoteManager();
        notesPath = Path.Combine(Application.persistentDataPath, "notes");

        if (Directory.Exists(notesPath))
        {
            Directory.Delete(notesPath, true);
        }
    }

    [TearDown]
    public void TearDown()
    {
        if (Directory.Exists(notesPath))
        {
            Directory.Delete(notesPath, true);
        }
    }

    [Test]
    public void AddNote_ShouldAddToList()
    {
        var note = new Note("AddMe", "Content");
        manager.AddNote(note);

        Assert.Contains(note, manager.Notes);
    }

    [Test]
    public void DeleteNote_ShouldDeleteFromList_AndDeleteLocalFile()
    {
        var note = new Note("DeleteMe", "Content");
        manager.AddNote(note);
        manager.SaveNote(note);
        manager.DeleteNoteByTitle(note.Title);

        Assert.IsFalse(manager.Notes.Contains(note));
        string path = Path.Combine(notesPath, "DeleteMe.json");
        Assert.IsFalse(File.Exists(path));
    }

    [Test]
    public void EditNote_TitleAndContentShouldBeUpdated()
    {
        var note = new Note("EditMe", "original content");
        manager.AddNote(note);
        manager.EditNote(note, "EditedTitle", "new content");

        Assert.AreEqual("EditedTitle", note.Title);
        Assert.AreEqual("new content", note.Content);
    }

    [Test]
    public void SaveNote_ShouldSaveAsJsonFile()
    {
        var note = new Note("SaveMe", "content");
        manager.AddNote(note);
        manager.SaveNote(note);

        string path = Path.Combine(notesPath, "SaveMe.json");
        Assert.IsTrue(File.Exists(path));
    }

    [Test]
    public void LoadNote_ShouldReturnNoteObject()
    {
        var note = new Note("LoadMe", "Load this note");
        manager.AddNote(note);
        manager.SaveNote(note);

        var loaded = manager.LoadNote("LoadMe");

        Assert.IsNotNull(loaded);
        Assert.AreEqual("LoadMe", loaded.Title);
        Assert.AreEqual("Load this note", loaded.Content);
    }

}
