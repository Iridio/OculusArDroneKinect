using OculusParrotKinect.Drone.Infrastructure;
using OculusParrotKinect.Drone.Data;

namespace OculusParrotKinect.Drone.Video
{
  public class VideoPacketDecoder : DisposableBase
  {
    private readonly PixelFormat _pixelFormat;
    private VideoConverter _videoConverter;
    private VideoDecoder _videoDecoder;
    private AVFrame _avFrame;
    private AVPacket _avPacket;

    public VideoPacketDecoder(PixelFormat pixelFormat)
    {
      _pixelFormat = pixelFormat;
      _avFrame = new AVFrame();
      _avPacket = new AVPacket();
    }

    public unsafe bool TryDecode(ref VideoPacket packet, out VideoFrame frame)
    {
      if (_videoDecoder == null)
        _videoDecoder = new VideoDecoder();
      fixed (byte* pData = &packet.Data[0])
      {
        _avPacket.data = pData;
        _avPacket.size = packet.Data.Length;
        if (_videoDecoder.TryDecode(ref _avPacket, ref _avFrame))
        {
          if (_videoConverter == null)
            _videoConverter = new VideoConverter(_pixelFormat.ToAVPixelFormat());
          byte[] data = _videoConverter.ConvertFrame(ref _avFrame);
          frame = new VideoFrame()
          {
            Timestamp = packet.Timestamp,
            Number = packet.FrameNumber,
            Width = packet.Width,
            Height = packet.Height,
            Depth = data.Length / (packet.Width * packet.Height),
            PixelFormat = _pixelFormat,
            Data = data
          };
          return true;
        }
      }
      frame = null;
      return false;
    }

    protected override void DisposeOverride()
    {
      if (_videoDecoder != null)
        _videoDecoder.Dispose();
      if (_videoConverter != null)
        _videoConverter.Dispose();
    }
  }
}