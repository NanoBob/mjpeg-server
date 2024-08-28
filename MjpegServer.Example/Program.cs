using MjpegServer;
using MjpegServer.Encoding;
using MjpegServer.Sources;

var port = 50808;

var video = new CameraVideoSource();
var streamer = new MjpegStreamer(video, new WindowsJpegEncoder(60), port);

streamer.Start();
Console.WriteLine($"Started on http://127.0.0.1:{port}");

await Task.Delay(-1);
