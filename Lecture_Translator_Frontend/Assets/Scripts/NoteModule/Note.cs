using UnityEngine;
using System.Text.RegularExpressions;

/// <summary>
/// Class <c>Note</c> represents a note with a title and its content.
/// This class is marked as [System.Serializable] so it can be serialized by Unity's JsonUtility and displayed in the Inspector.
/// </summary>
[System.Serializable]
public class Note
{
    /// <summary>
    /// The title of the note.
    /// </summary>
    public string Title;

    /// <summary>
    /// The content of the note.
    /// </summary>
    public string Content;

    /// <summary>
    /// The lecture title this note is associated with.
    /// </summary>
    public string LectureTitle;

    /// <summary>
    /// The timestamp of the lecture when this note was created.
    /// </summary>
    public double CreatedAtSeconds;


    /// <summary>
    /// Constructor for deserialization of Unity instantiation.
    /// </summary>
    public Note() { }

    /// <summary>
    /// Creates a note with title and content.
    /// </summary>
    /// <param name="title">The title of the note.</param>
    /// <param name="content">The content of the note.</param>
    public Note(string title, string content)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new System.ArgumentException("Title cannot be empty.");
        }

        if (!IsValidTitle(title))
        {
            throw new System.ArgumentException("Title contains invalid characters. Only A-Z, a-z, 0-9, -, _ are allowed.)");
        }
        Title = title;
        Content = content;
    }

    /// <summary>
    /// Only numbers(0-9), letters(A-Z, a-z), hyphen(-) and underscores(_) are allowed.
    /// </summary>
    /// <param name="title">The title string to validate.</param>
    /// <returns>True if the title contains only allowed characters; Otherwise return false.</returns>
    private bool IsValidTitle(string title)
    {
        return Regex.IsMatch(title, @"^[A-Za-z0-9\-_]+$");
    }
}
