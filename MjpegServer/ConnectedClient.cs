using System.Collections.Concurrent;
using System.Net.Sockets;

namespace MjpegServer;

public class ConnectedClient
{
    private readonly Socket socket;

    private readonly object lastFrameLock = new();
    private byte[]? lastFrame;

    public bool IsConnected { get; private set; } = true;

    public ConnectedClient(Socket socket)
    {
        this.socket = socket;

        _ = Task.Run(Run);
    }

    public void EnqueueFrame(byte[] data)
    {
        if (!this.IsConnected)
            throw new Exception("Socket disconnected");

        socket.Send(data);
    }

    private async void Run()
    {
        try
        {
            while (IsConnected)
            {
                if (this.lastFrame != null)
                {
                    socket.Send(this.lastFrame);
                    this.lastFrame = null;
                }
                await Task.Delay(1);
            }
        }
        catch (SocketException)
        {
            IsConnected = false;
        }

    }
}
