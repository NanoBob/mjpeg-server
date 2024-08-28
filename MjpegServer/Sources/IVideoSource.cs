namespace MjpegServer.Sources;

public interface IVideoSource
{
    event EventHandler<FrameAvailableEventArgs>? FrameAvailable;

    void Start();
    void Stop();
}
