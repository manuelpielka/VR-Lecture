using System;
using System.IO.Enumeration;
using UnityEngine;
using UnityEngine.Android;

[CreateAssetMenu(fileName = "NewEnvironment", menuName = "VR/EnvironmentConfiguration")]

/// <summary>
/// Class <c>EnvironmentConfig</c> stores configuration for a single virtual environment.
/// </summary>
public class EnvironmentConfig : ScriptableObject
{
    /// <summary>
    /// The scene ID used by the system to locate the environment.
    /// </summary>
    [Tooltip("A unique ID used to identify and switch between environments.")]
    public string SceneId;

    /// <summary>
    /// The display name shown to users in dropdowns and UI.
    /// </summary>
    [Tooltip("Name shown to the user in the UI.")]
    public string DisplayName;

    /// <summary>
    /// The actual prefab taht gets instantiated for the background environment.
    /// </summary>
    [Tooltip("The prefab that represents the 3D background environment.")]
    public GameObject BackgroundPrefab;
}