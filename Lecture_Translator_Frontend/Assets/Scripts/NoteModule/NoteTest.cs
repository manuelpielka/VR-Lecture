using UnityEngine;

public class NoteTest : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        TestAddNote();
        TestSaveNote();
        TestLoadNote();
        TestEditNote();
        TestDeleteNote();
    }

    // Update is called once per frame
    void Update()
    {

    }

    void TestAddNote()
    {
        Debug.Log("Running TestAddNote...");
        NoteManager manager = new NoteManager();
        Note note = new Note("Note1", "Content");
        manager.AddNote(note);
        Debug.Log($"Note count: {manager.Notes.Count}");
    }

    void TestSaveNote()
    {
        Debug.Log("Running TestSaveNote...");
        NoteManager manager = new NoteManager();
        Note note = new Note("SaveTest", "To be saved");
        manager.AddNote(note);
        manager.SaveNote(note);
    }

    void TestLoadNote()
    {
        Debug.Log("Running TestLoadNote...");
        NoteManager manager = new NoteManager();
        Note loaded = manager.LoadNote("SaveTest");

        if (loaded == null)
        {
            Debug.LogError("❌ LoadNote returned null!");
        }
        else
        {
            Debug.Log($"✅ Loaded note: Title = {loaded.Title}, Content = {loaded.Content}");
        }


        Debug.Log($"Loaded note content: {loaded?.Content}");
    }

    void TestEditNote()
    {
        Debug.Log("Running TestEditNote...");
        NoteManager manager = new NoteManager();
        Note note = new Note("EditTest", "Before edit");
        manager.AddNote(note);
        manager.EditNote(note, "EditedTest", "After edit");
        Debug.Log($"New title: {note.Title}, New content: {note.Content}");
    }

    void TestDeleteNote()
    {
        Debug.Log("Running TestDeleteNote...");
        NoteManager manager = new NoteManager();
        Note note = new Note("DeleteTest", "To be deleted");
        manager.AddNote(note);
        manager.SaveNote(note);
        manager.DeleteNote(note);
    }

    void 

}
