using OpenCvSharp;

namespace MjpegServer.Encoding;

public interface IJpegEncoder
{
    byte[] EncodeFrame(Mat frame);
}
