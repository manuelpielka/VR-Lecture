/// <summary>
/// Interface that represents a sse handler.
/// </summary>
public interface ISSEHandler
{
    /// <summary>
    /// Event received when the sse connection is opened.
    /// </summary>
    void OnSSEConnectionOpened();
    /// <summary>
    /// Event received when the sse connection is closed.
    /// </summary>
    void OnSSEConnectionClosed();
    /// <summary>
    /// Event received when the sse connection receives a message.
    /// </summary>
    void OnSSEEventReceived(string eventName, string data);
    /// <summary>
    /// Event received when the sse connection has an error.
    /// </summary>
    void OnSSEError(System.Exception ex);
}
