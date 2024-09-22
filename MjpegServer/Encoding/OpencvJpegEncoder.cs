using OpenCvSharp;

namespace MjpegServer.Encoding;

public class OpencvJpegEncoder : IJpegEncoder
{
    public byte[] EncodeFrame(Mat frame)
    {
        var data = frame.ImEncode(".jpg", [(int)ImwriteFlags.JpegQuality, 100]);

        return data;
    }
}
