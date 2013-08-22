using System;

namespace OculusParrotKinect.Drone.Video.Exceptions
{
  public class VideoDecoderException : Exception
  {
    public VideoDecoderException(string message)
      : base(message)
    {
    }
  }
}