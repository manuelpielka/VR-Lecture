using System.Collections.Generic;

public static class WindowKeys
{
    public const string MainMenuKey = "MainMenuWindow";
    public const string LectureBrowserKey = "LectureBrowserWindow";
    public const string LecturePlayerKey = "LecturePlayerWindow";
    public const string NotesKey = "NotesWindow";
    public const string LectureNoteKey = "LectureNoteWindow";
    public const string CreateNoteKey = "CreateNoteWindow";
    public const string CreateLectureNoteKey = "CreateLectureNoteWindow";
    public const string TranscriptKey = "TranscriptWindow";
    public const string DialogueKey = "DialogueWindow";
    public const string SettingsKey = "SettingsWindow";
    public static List<string> AllKeys { get; set; } = new List<string>
    {
        MainMenuKey,
        LectureBrowserKey,
        LecturePlayerKey,
        NotesKey,
        LectureNoteKey,
        CreateNoteKey,
        CreateLectureNoteKey,
        TranscriptKey,
        DialogueKey,
        SettingsKey,
    };  
}