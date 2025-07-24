using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// A SSEClient is used to connect to a server via sse and receive a data stream from it.
/// </summary>
public class SSEClient
{
    /// <summary>
    /// The url of the server.
    /// </summary>
    private readonly string url;

    /// <summary>
    /// The sseHandler to send events to.
    /// </summary>
    private readonly ISSEHandler sseHandler;

    /// <summary>
    /// Cancellation token to stop the data stream.
    /// </summary>
    private CancellationTokenSource cancellationTokenSource;

    /// <summary>
    /// Used to run the sse connection.
    /// </summary>
    private Task sseTask;

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="url"> Url of the server. </param>
    /// <param name="sseHandler"> Handler to send events to. </param>
    public SSEClient(string url, ISSEHandler sseHandler)
    {
        this.url = url;
        this.sseHandler = sseHandler;
    }

    /// <summary>
    /// Initialises the sse connection.
    /// </summary>
    public void InitSse()
    {
        cancellationTokenSource = new CancellationTokenSource();
        sseTask = Task.Run(() => ConnectSSE(cancellationTokenSource.Token));
    }

    /// <summary>
    /// Connects to the sse server and waits to receive data.
    /// </summary>
    /// <param name="cancellationToken"> Cancellation token to stop the data stream. </param>
    /// <returns> An awaitable task. </returns>
    private async Task ConnectSSE(CancellationToken cancellationToken)
    {
        Debug.Log("CONNECTING...");
        try
        {
            using (var client = new HttpClient())
            {
                var request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.Accept.ParseAdd("text/event-stream");

                using (var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken))
                {
                    if (!response.IsSuccessStatusCode)
                    {
                        throw new Exception($"Failed to connect. Status: {response.StatusCode}");
                    }

                    sseHandler.OnSSEConnectionOpened();

                    var stream = await response.Content.ReadAsStreamAsync();
                    using (var reader = new System.IO.StreamReader(stream))
                    {
                        string eventName = "message";
                        string dataBuffer = "";

                        while (!reader.EndOfStream && !cancellationToken.IsCancellationRequested)
                        {
                            Debug.Log("Waiting for line...");
                            var line = await reader.ReadLineAsync();

                            if (string.IsNullOrEmpty(line))
                            {
                                if (!string.IsNullOrEmpty(dataBuffer))
                                {
                                    sseHandler.OnSSEEventReceived(eventName, dataBuffer.Trim());
                                    dataBuffer = "";
                                    eventName = "message";
                                }
                                continue;
                            }

                            if (line.StartsWith("event:"))
                            {
                                eventName = line.Substring("event:".Length).Trim();
                            }
                            else if (line.StartsWith("data:"))
                            {
                                dataBuffer += line.Substring("data:".Length).Trim() + "\n";
                            }
                        }
                    }
                }

                sseHandler.OnSSEConnectionClosed();
            }
        }
        catch (Exception ex)
        {
            sseHandler.OnSSEError(ex);
        }
    }

    /// <summary>
    /// Disconnects the sse connection.
    /// </summary>
    public void Disconnect()
    {
        cancellationTokenSource?.Cancel();
    }
}
