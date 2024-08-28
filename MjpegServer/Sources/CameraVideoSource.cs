using OpenCvSharp;
using System.Diagnostics;

namespace MjpegServer.Sources;

public class CameraVideoSource(VideoCapture captureDevice) : IVideoSource
{
    private readonly VideoCapture captureDevice = captureDevice;
    private readonly Mat frame = new();

    private readonly TimeSpan frameTime;

    private bool running = false;

    public CameraVideoSource(int index = 0, int width = 1920, int height = 1080, int fps = 30)
        : this(new VideoCapture(index, VideoCaptureAPIs.DSHOW))
    {
        captureDevice.Set(VideoCaptureProperties.FrameWidth, width);
        captureDevice.Set(VideoCaptureProperties.FrameHeight, height);
        captureDevice.Set(VideoCaptureProperties.Fps, fps);
        frameTime = TimeSpan.FromMilliseconds(1000f / fps);
    }

    public void Start()
    {
        running = true;
        _ = Task.Run(GrabLoop);
    }

    public void Stop()
    {
        running = false;
    }

    private async void GrabLoop()
    {
        while (running)
        {
            captureDevice.Grab();
            captureDevice.Retrieve(frame);
            FrameAvailable?.Invoke(this, new FrameAvailableEventArgs(frame));
            await Task.Delay(frameTime);
        }
    }

    public event EventHandler<FrameAvailableEventArgs>? FrameAvailable;
}

public class FrameAvailableEventArgs(Mat frame) : EventArgs
{
    public Mat Frame { get; } = frame;
}
