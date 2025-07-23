using System;

/// <summary>
/// Serves as the interface for all tutorial steps.
/// </summary>
public interface ITutorialStep
{
    event Action StepCompleted;
    /// <summary>
    /// Method decleration for starting a tutorial step.
    /// </summary>
    public void StartStep();

    /// <summary>
    /// Method declaration for updating a tutorial step.
    /// </summary>
    ///public void UpdateStep();

    /// <summary>
    /// Method declaration for ending a tutorial step.
    /// </summary>
    public void EndStep();

}
