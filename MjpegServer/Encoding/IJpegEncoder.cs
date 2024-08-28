using OpenCvSharp;

namespace MjpegServer.Encoding;

public interface IJpegEncoder
{
    MemoryStream EncodeFrame(Mat frame);
}
