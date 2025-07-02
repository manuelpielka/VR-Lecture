/// <summary>
/// Serves as the interface for all tutorial steps.
/// </summary>
public interface ITutorialStep
{
    /// <summary>
    /// Method decleration for starting a tutorial step.
    /// </summary>
    public void StartStep();

    /// <summary>
    /// Method declaration for updating a tutorial step.
    /// </summary>
    public void UpdateStep();

    /// <summary>
    /// Method declaration for ending a tutorial step.
    /// </summary>
    public void EndStep();

    /// <summary>
    /// Method declaration for checking if the tutorial step is complete.
    /// </summary>
    /// <returns>Returns if the step is already completed. </returns>
    public bool GetIsComplete();
}
