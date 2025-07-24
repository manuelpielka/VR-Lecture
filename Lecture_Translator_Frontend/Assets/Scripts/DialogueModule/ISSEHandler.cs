public interface ISSEHandler
{
    void OnSSEConnectionOpened();
    void OnSSEConnectionClosed();
    void OnSSEEventReceived(string eventName, string data);
    void OnSSEError(System.Exception ex);
}
