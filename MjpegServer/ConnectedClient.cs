using System.Collections.Concurrent;
using System.Net.Sockets;

namespace MjpegServer;

public class ConnectedClient
{
    private readonly ConcurrentQueue<byte[]> frames = [];
    private readonly Socket socket;

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

        this.frames.Enqueue(data);
    }

    private async void Run()
    {
        try
        {
            while (IsConnected)
            {
                while (this.frames.TryDequeue(out var frame))
                {
                    socket.Send(frame);
                }
            }
        }
        catch (SocketException)
        {
            IsConnected = false;
        }

    }
}
