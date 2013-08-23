using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Kinect;
using Microsoft.Speech.AudioFormat;
using Microsoft.Speech.Recognition;
using OculusParrotKinect.Kinect.Events;

namespace OculusParrotKinect.Kinect
{
  public class KinectClient : IDisposable
  {
    public string StatusMessage { get; private set; }

    private KinectSensor sensor;
    private SpeechRecognitionEngine speechEngine;

    public enum GestureCommandType { Hover, StrafeLeft, StrafeRight, Forward, Backward, StrafeForwardLeft, StrafeForwardRigth, StrafeBackwardLeft, StrafeBackwardRight, NoPlayerDetected }
    public EventHandler<GestureCommandRecognizedEventArgs> GestureCommandRecognized;
    protected virtual void OnGestureCommandRecognized(GestureCommandRecognizedEventArgs e)
    {
      if (GestureCommandRecognized != null)
        GestureCommandRecognized(this, e);
    }

    public enum VoiceCommandType { TakeOff, Land, Emergency, ChangeCamera };
    public EventHandler<VoiceCommandRecognizedEventArgs> VoiceCommandRecognized;
    public EventHandler VoiceCommandRejected;
    protected virtual void OnVoiceCommandRecognized(VoiceCommandRecognizedEventArgs e)
    {
      if (VoiceCommandRecognized != null)
        VoiceCommandRecognized(this, e);
    }

    protected virtual void OnVoiceCommandRejected()
    {
      if (VoiceCommandRejected != null)
        VoiceCommandRejected(this, new EventArgs());
    }

    public void Dispose()
    {
      if (sensor != null)
      {
        sensor.SkeletonFrameReady -= SkeletonFrameReady;
        sensor.AudioSource.Stop();
        sensor.Stop();
        sensor = null;
      }
      if (speechEngine != null)
      {
        speechEngine.SpeechRecognized -= SpeechRecognized;
        speechEngine.SpeechRecognitionRejected -= SpeechRejected;
        speechEngine.RecognizeAsyncStop();
      }
    }

    private void SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
    {
      if (e.Result.Confidence >= 0.6f)
      {
        switch (e.Result.Semantics.Value.ToString())
        {
          case VoiceCommands.TakeOff:
            OnVoiceCommandRecognized(new VoiceCommandRecognizedEventArgs(VoiceCommandType.TakeOff));
            break;
          case VoiceCommands.Land:
            OnVoiceCommandRecognized(new VoiceCommandRecognizedEventArgs(VoiceCommandType.Land));
            break;
          case VoiceCommands.Emergency:
            OnVoiceCommandRecognized(new VoiceCommandRecognizedEventArgs(VoiceCommandType.Emergency));
            break;
          case VoiceCommands.ChangeCamera:
            OnVoiceCommandRecognized(new VoiceCommandRecognizedEventArgs(VoiceCommandType.ChangeCamera));
            break;
        }
      }
    }

    private void SpeechRejected(object sender, SpeechRecognitionRejectedEventArgs e)
    {
      OnVoiceCommandRejected();
    }

    public KinectClient(string culture)
    {
      foreach (var potentialSensor in KinectSensor.KinectSensors)
      {
        if (potentialSensor.Status == KinectStatus.Connected)
        {
          sensor = potentialSensor;
          break;
        }
      }
      if (sensor != null)
      {
        try
        {
          sensor.SkeletonStream.Enable(new TransformSmoothParameters() { Smoothing = 0.5f, Correction = 0.5f, Prediction = 0.5f, JitterRadius = 0.05f, MaxDeviationRadius = 0.04f });
          sensor.SkeletonFrameReady += SkeletonFrameReady;
          sensor.Start();
        }
        catch (IOException)
        {
          sensor = null;
        }
      }
      if (sensor == null)
        throw new Exception("Kinect not found.");

      RecognizerInfo recognizerInfo = GetKinectRecognizer(culture);
      if (recognizerInfo != null)
      {
        speechEngine = new SpeechRecognitionEngine(recognizerInfo.Id);
        speechEngine.LoadGrammar(VoiceCommands.GetCommandsGrammar(recognizerInfo.Culture));
        speechEngine.SpeechRecognized += SpeechRecognized;
        speechEngine.SpeechRecognitionRejected += SpeechRejected;
        speechEngine.SetInputToAudioStream(sensor.AudioSource.Start(), new SpeechAudioFormatInfo(EncodingFormat.Pcm, 16000, 16, 1, 32000, 2, null));
        speechEngine.RecognizeAsync(RecognizeMode.Multiple);
      }
      else
        throw new Exception("Kinect not found.");
    }

    private static RecognizerInfo GetKinectRecognizer(string culture)
    {
      foreach (RecognizerInfo recognizer in SpeechRecognitionEngine.InstalledRecognizers())
      {
        string value;
        recognizer.AdditionalInfo.TryGetValue("Kinect", out value);
        if ("True".Equals(value, StringComparison.OrdinalIgnoreCase) && culture.Equals(recognizer.Culture.Name, StringComparison.OrdinalIgnoreCase))
          return recognizer;
      }
      return null;
    }

    bool IsPlayerActive;
    void SkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
    {
      using (SkeletonFrame skeletonFrame = e.OpenSkeletonFrame())
      {
        if (skeletonFrame != null)
        {
          Skeleton[] skeletonData = new Skeleton[skeletonFrame.SkeletonArrayLength];
          skeletonFrame.CopySkeletonDataTo(skeletonData);
          Skeleton playerSkeleton = (from s in skeletonData where s.TrackingState == SkeletonTrackingState.Tracked select s).FirstOrDefault();
          IsPlayerActive = playerSkeleton != null;
          if (IsPlayerActive)
          {
            var spine = playerSkeleton.Joints[JointType.Spine];
            var handRight = playerSkeleton.Joints[JointType.HandRight];
            var handLeft = playerSkeleton.Joints[JointType.HandLeft];
            var elbowLeft = playerSkeleton.Joints[JointType.ElbowLeft];
            var elbowRight = playerSkeleton.Joints[JointType.ElbowRight];
            var hip = playerSkeleton.Joints[JointType.HipCenter];
            //for debug
            StatusMessage = String.Format("LH z:{0} - S X:{1} ", handLeft.Position.Z, spine.Position.Z);

            bool left = handLeft.Position.Y > hip.Position.Y && handLeft.Position.X < elbowLeft.Position.X && ((elbowLeft.Position.X - handLeft.Position.X) > -0.2);
            bool right = handRight.Position.Y > hip.Position.Y && handRight.Position.X > elbowRight.Position.X && ((handRight.Position.X - elbowRight.Position.X) > 0.2);
            bool forwardLeft = handLeft.Position.Y > hip.Position.Y && spine.Position.Z - handLeft.Position.Z > 0.5;
            bool forwardRight = handRight.Position.Y > hip.Position.Y && spine.Position.Z - handRight.Position.Z > 0.5;
            bool backwardLeft = handLeft.Position.Y > hip.Position.Y && spine.Position.Z - handLeft.Position.Z < 0.2;
            bool backwardRight = handRight.Position.Y > hip.Position.Y && spine.Position.Z - handRight.Position.Z < 0.2;

            if (left && !forwardRight && !right && !backwardRight)
            {
              OnGestureCommandRecognized(new GestureCommandRecognizedEventArgs(GestureCommandType.StrafeLeft));
              return;
            }
            if (!left && !forwardLeft && right && !backwardLeft)
            {
              OnGestureCommandRecognized(new GestureCommandRecognizedEventArgs(GestureCommandType.StrafeRight));
              return;
            }
            if (left && forwardRight && !right)
            {
              OnGestureCommandRecognized(new GestureCommandRecognizedEventArgs(GestureCommandType.StrafeForwardLeft));
              return;
            }
            if (left && backwardRight && !right)
            {
              OnGestureCommandRecognized(new GestureCommandRecognizedEventArgs(GestureCommandType.StrafeBackwardLeft));
              return;
            }
            if (forwardLeft && forwardRight && !left && !right)
            {
              OnGestureCommandRecognized(new GestureCommandRecognizedEventArgs(GestureCommandType.Forward));
              return;
            }
            if (right && forwardLeft && !left)
            {
              OnGestureCommandRecognized(new GestureCommandRecognizedEventArgs(GestureCommandType.StrafeForwardRigth));
              return;
            }
            if (right && backwardLeft && !left)
            {
              OnGestureCommandRecognized(new GestureCommandRecognizedEventArgs(GestureCommandType.StrafeBackwardRight));
              return;
            }
            if (backwardRight && backwardLeft && !left && !right)
            {
              OnGestureCommandRecognized(new GestureCommandRecognizedEventArgs(GestureCommandType.Backward));
              return;
            }
            OnGestureCommandRecognized(new GestureCommandRecognizedEventArgs(GestureCommandType.Hover));
            return;
          }
          else
            OnGestureCommandRecognized(new GestureCommandRecognizedEventArgs(GestureCommandType.NoPlayerDetected));
        }
      }
    }
  }
}
