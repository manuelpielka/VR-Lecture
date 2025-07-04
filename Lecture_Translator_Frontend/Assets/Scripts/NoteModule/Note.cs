using UnityEngine;

/// <summary>
/// Class <c>Note</c> represents a note with a title and its content.
/// </summary>
public class Note
{
    /// <summary>
    /// The title of the note.
    /// </summary>
    public string Title { get; set; }

    /// <summary>
    /// The content of the note.
    /// </summary>
    public string Content { get; set; }

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
        Title = title;
        Content = content;
    }
}
