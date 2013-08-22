using System;
using System.Collections.Generic;
using System.Linq;

namespace OculusParrotKinect.Kinect.Events
{
  public class VoiceCommandRecognizedEventArgs : EventArgs
  {
    public KinectClient.VoiceCommandType VoiceCommand { get; private set; }
    public VoiceCommandRecognizedEventArgs(KinectClient.VoiceCommandType command)
    {
      VoiceCommand = command;
    }
  }
}