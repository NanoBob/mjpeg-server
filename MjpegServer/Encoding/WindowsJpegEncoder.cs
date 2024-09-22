using OpenCvSharp;
using OpenCvSharp.Extensions;
using System.Drawing.Imaging;
using System.Runtime.Versioning;

namespace MjpegServer.Encoding;

[SupportedOSPlatform("windows6.1")]
public class WindowsJpegEncoder : IJpegEncoder, IDisposable
{
    private readonly ImageCodecInfo encoder = ImageCodecInfo.GetImageEncoders()
        .First(c => c.FormatID == ImageFormat.Jpeg.Guid);

    private readonly EncoderParameters parameters;
    private readonly EncoderParameter quality;

    public WindowsJpegEncoder(long quality = 100)
    {
        this.parameters = new EncoderParameters(1);
        this.quality = new EncoderParameter(Encoder.Quality, quality);
        this.parameters.Param[0] = this.quality;
    }

    ~WindowsJpegEncoder()
    {
        Dispose();
    }

    public void Dispose()
    {
        this.quality.Dispose();
        this.parameters.Dispose();
        GC.SuppressFinalize(this);
    }

    public byte[] EncodeFrame(Mat frame)
    {
        using var frameStream = new MemoryStream();

        var image = frame.ToBitmap();
        image.Save(frameStream, this.encoder, this.parameters);

        var buffer = new byte[frameStream.Length];
        frameStream.Read(buffer, 0, buffer.Length);

        return buffer;
    }
}
