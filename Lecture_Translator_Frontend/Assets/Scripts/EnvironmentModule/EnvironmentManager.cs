using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Class <c>EnvironmentManager</c> manages switching between different VR environments.
/// </summary>
public class EnvironmentManager : MonoBehaviour
{
    //A list of all available VR environment configurations.
    [SerializeField]
    public List<EnvironmentConfig> Environments = new List<EnvironmentConfig>();

    
    //The currently active environment configuration.
    public EnvironmentConfig CurrentEnvironment{ get; private set; }
    
    [SerializeField] private Transform backgroundContainer;
    private GameObject currentBackgroundInstance;


    //Loads the environment configuration using the given scene ID and initiates the switch to that environment.
    public void LoadEnvironment(string sceneId)
    {
        // Look up the environment configuration that matches the given scene ID
        var config = FindEnvironmentById(sceneId);
        if (config != null)
        {
            // If found, switch to the selected environment
            SwitchEnvironment(config);
        }
        else
        {
            // If not found, print a warning message
            Debug.LogWarning($"Environment: '{sceneId}' not found.");
        }
    }

    //Switches to the specified environment by loading its scene and applying its config- uration.
    public void SwitchEnvironment(EnvironmentConfig config)
    {
        if (config == null)
        {
            return;
        }

        if (currentBackgroundInstance != null)
        {
            // Removes the previous background if it exists.
            Destroy(currentBackgroundInstance);
        }
        // Instantiate the new background and assign it as the current one.
        currentBackgroundInstance = Instantiate(config.BackgroundPrefab, backgroundContainer);
        CurrentEnvironment = config;
    }

    //Searches available environments and returns the one matching the given scene ID.
    public EnvironmentConfig FindEnvironmentById(string sceneId)
    {
        // Finds and returns the environment that matches the given SceneId.
        return Environments.Find(environment => environment.SceneId == sceneId);
    }



    /// <summary>
    /// Called when the scene starts. 
    /// Automatically loads the first environment in the list, so the user enters the app with a default VR background.
    /// </summary>
    void Start()
    {
        // If the list of environments is not empty, automatically load the first environment.
        if (Environments.Count > 0)
        {
            LoadEnvironment(Environments[0].SceneId);
        }
    }

    // Update is called once per frame
    //void Update(){}
}
