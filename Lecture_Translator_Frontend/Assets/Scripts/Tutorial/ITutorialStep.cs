using System;

/// <summary>
/// Serves as the interface for all tutorial steps.
/// </summary>
public interface ITutorialStep
{
    /// <summary>
    /// Event that is triggered when the step is completed.
    /// </summary>
    event Action StepCompleted;



    /// <summary>
    /// Method decleration for starting a tutorial step.
    /// </summary>
    public void StartStep();


    /// <summary>
    /// Method declaration for ending a tutorial step.
    /// </summary>
    public void EndStep();

}
