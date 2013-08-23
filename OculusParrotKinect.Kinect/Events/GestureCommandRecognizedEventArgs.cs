using System;

namespace OculusParrotKinect.Kinect.Events
{
  public class GestureCommandRecognizedEventArgs : EventArgs
  {
    public KinectClient.GestureCommandType GestureCommand { get; private set; }
    public GestureCommandRecognizedEventArgs(KinectClient.GestureCommandType command)
    {
      GestureCommand = command;
    }
  }
}
