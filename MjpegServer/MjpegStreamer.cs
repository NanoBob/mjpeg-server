using MjpegServer.Encoding;
using MjpegServer.Sources;
using System.Net;
using System.Net.Sockets;

namespace MjpegServer;

public class MjpegStreamer
{
    private readonly IVideoSource videoSource;
    private readonly IJpegEncoder jpegEncoder;

    private readonly TcpListener server;
    private bool running;

    private readonly List<ConnectedClient> connectedSockets = [];

    private const string boundary = "--BOUNDARY--";
    private const string firstResponseHeaderText = $"""
        HTTP/1.1 200 OK
        Content-Type: multipart/x-mixed-replace; boundary={boundary}
        Server: MjpegServer

        """;

    private readonly byte[] firstResponseHeader = System.Text.Encoding.UTF8.GetBytes(firstResponseHeaderText);


    public MjpegStreamer(IVideoSource videoSource, IJpegEncoder jpegEncoder, int port)
    {
        this.videoSource = videoSource;
        this.jpegEncoder = jpegEncoder;

        this.server = new TcpListener(IPAddress.Any, port);

        videoSource.FrameAvailable += BroadcastNewFrame;
    }

    public void Start()
    {
        this.server.Start();
        this.videoSource.Start();

        this.running = true;

        ConnectionLoop();
    }

    public void Stop()
    {
        this.running = false;
        this.videoSource.Stop();
        this.server.Stop();
    }

    private async void ConnectionLoop()
    {
        while (this.running)
        {
            var socket = await this.server.AcceptSocketAsync();

            socket.Send(firstResponseHeader);
            this.connectedSockets.Add(new ConnectedClient(socket));
        }
    }

    private void BroadcastNewFrame(object? sender, FrameAvailableEventArgs e)
    {
        try
        {
            var image = this.jpegEncoder.EncodeFrame(e.Frame);

            var header = $"""

                {boundary}
                Content-Type: image/jpeg
                Content-Length: {image.Length}


            """;

            var fullFrame = System.Text.Encoding.UTF8.GetBytes(header)
                    .Concat(image.GetBuffer())
                    .Concat(System.Text.Encoding.UTF8.GetBytes("\r\n"))
                    .ToArray();

            var sockets = this.connectedSockets.ToArray();
            foreach (var socket in sockets)
            {
                try
                {
                    socket.EnqueueFrame(fullFrame);
                }
                catch (SocketException socketException)
                {
                    this.connectedSockets.Remove(socket);
                    Console.WriteLine($"Disconnected\n{socketException.Message}");
                }
            }
        }
        catch (Exception exception)
        {
            Console.WriteLine(exception.Message);
        }
    }
}
