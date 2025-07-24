using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class SSEClient
{
    private readonly string url;
    private readonly ISSEHandler sseHandler;
    private CancellationTokenSource cancellationTokenSource;
    private Task sseTask;

    public SSEClient(string url, ISSEHandler sseHandler)
    {
        this.url = url;
        this.sseHandler = sseHandler;
    }

    public void InitSse()
    {
        cancellationTokenSource = new CancellationTokenSource();
        sseTask = Task.Run(() => ConnectSSE(cancellationTokenSource.Token));
    }

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

    public void Disconnect()
    {
        cancellationTokenSource?.Cancel();
    }
}
