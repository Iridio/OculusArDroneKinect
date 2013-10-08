using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using OculusParrotKinect.Drone;
using OculusParrotKinect.Drone.Configuration;
using OculusParrotKinect.Drone.Configuration.Sections;
using OculusParrotKinect.Drone.Data;
using OculusParrotKinect.Drone.Video;
using OculusParrotKinect.Kinect;
using OculusParrotKinect.Kinect.Events;
using OculusParrotKinect.Oculus;

namespace OculusParrotKinect
{
  /// <summary>
  /// This is the main type for your game
  /// </summary>
  public class OculusParrotKinect : Game
  {
    bool goFullScreen = false;
    bool drawOculus = true;
    bool drawTestImage = false;
    float scaleImageFactor = 1.0f;

    GraphicsDeviceManager graphics;
    SpriteBatch spriteBatch;
    Effect oculusRiftDistortionShader;
    SpriteFont hudFontSmall;

    string kinectMessage = string.Empty;
    string kinectStatusMessage = string.Empty;
    string lastCommandSent = String.Empty;
    enum ScreenType { Splash, Drive }
    ScreenType screenType;

    public OculusParrotKinect()
    {
      graphics = new GraphicsDeviceManager(this);
      Content.RootDirectory = "Content";
      graphics.IsFullScreen = goFullScreen;
      graphics.PreferredBackBufferHeight = 800;
      graphics.PreferredBackBufferWidth = 1280;
    }

    #region VoiceCommands
    private void OnVoiceCommandRejected(object sender, EventArgs e)
    {
      kinectMessage = "Messaggio non riconosciuto";
    }

    private void OnVoiceCommandRecognized(object sender, VoiceCommandRecognizedEventArgs e)
    {
      if (screenType == ScreenType.Drive)
      {
        switch (e.VoiceCommand)
        {
          case KinectClient.VoiceCommandType.TakeOff:
            DroneTakeOff();//Qui tutti i comandi che servono
            kinectMessage = "Take off";
            break;
          case KinectClient.VoiceCommandType.Land: //qui lancio sempre il comando non serve verificare se è doppio
            droneClient.Land();
            kinectMessage = "Land";
            break;
          case KinectClient.VoiceCommandType.Emergency:
            droneClient.Emergency();
            kinectMessage = "Emergency";
            break;
          case KinectClient.VoiceCommandType.ChangeCamera:
            droneClient.Send(droneClient.Configuration.Video.Channel.Set(VideoChannelType.Next).ToCommand());
            kinectMessage = "Change camera";
            break;
        }
      }
    }
    #endregion

    #region GestureCommands
    private void OnGestureRecognized(object sender, GestureCommandRecognizedEventArgs e)
    {
      if (screenType == ScreenType.Drive)
      {
        switch (e.GestureCommand)
        {
          case KinectClient.GestureCommandType.Hover:
            kinectMessage = "Hover";
            if (lastCommandSent != "hover")
            {
              lastCommandSent = "hover";
              droneClient.Progress(Drone.Commands.FlightMode.Progressive, 0, 0, 0, 0);
            }
            break;
          case KinectClient.GestureCommandType.StrafeLeft:
            kinectMessage = "Strafe Left";
            if (lastCommandSent != "strafeleft")
            {
              lastCommandSent = "strafeleft";
              droneClient.Progress(Drone.Commands.FlightMode.Progressive, roll: -0.07f);
            }
            break;
          case KinectClient.GestureCommandType.StrafeRight:
            kinectMessage = "Strafe Right";
            if (lastCommandSent != "straferight")
            {
              lastCommandSent = "straferight";
              droneClient.Progress(Drone.Commands.FlightMode.Progressive, roll: 0.07f);
            }
            break;
          case KinectClient.GestureCommandType.Forward:
            kinectMessage = "Forward";
            if (lastCommandSent != "forward")
            {
              lastCommandSent = "forward";
              droneClient.Progress(Drone.Commands.FlightMode.Progressive, pitch: -0.1f);
            }
            break;
          case KinectClient.GestureCommandType.Backward:
            kinectMessage = "Backward";
            if (lastCommandSent != "backward")
            {
              lastCommandSent = "backward";
              droneClient.Progress(Drone.Commands.FlightMode.Progressive, pitch: 0.1f);
            }
            break;
          case KinectClient.GestureCommandType.StrafeForwardLeft:
            kinectMessage = "Strafe forward Left";
            if (lastCommandSent != "strafefwdleft")
            {
              lastCommandSent = "strafefwdleft";
              droneClient.Progress(Drone.Commands.FlightMode.Progressive, roll: -0.1f, pitch: 0.1f);
            }
            break;
          case KinectClient.GestureCommandType.StrafeForwardRigth:
            kinectMessage = "Strafe forward Right";
            if (lastCommandSent != "strafefwdright")
            {
              lastCommandSent = "strafefwdright";
              droneClient.Progress(Drone.Commands.FlightMode.Progressive, roll: 0.2f, pitch: 0.1f);
            }
            break;
          case KinectClient.GestureCommandType.StrafeBackwardLeft:
            kinectMessage = "Strafe backward Left";
            if (lastCommandSent != "strafebwdleft")
            {
              lastCommandSent = "strafebwdleft";
              droneClient.Progress(Drone.Commands.FlightMode.Progressive, roll: -0.2f, pitch: -0.1f);
            }
            break;
          case KinectClient.GestureCommandType.StrafeBackwardRight:
            kinectMessage = "Strafe backward Right";
            if (lastCommandSent != "strafebwdright")
            {
              lastCommandSent = "strafebwdright";
              droneClient.Progress(Drone.Commands.FlightMode.Progressive, roll: 0.2f, pitch: -0.1f);
            }
            break;
          case KinectClient.GestureCommandType.NoPlayerDetected:
            kinectMessage = "No player detected";
            if (lastCommandSent != "hover")
            {
              lastCommandSent = "hover";
              droneClient.Progress(Drone.Commands.FlightMode.Progressive, 0, 0, 0, 0);
            }
            break;
        }
        OculusHandle(); //TODO: move from here and put it in its own event handler
      }
    }
    #endregion

    protected override void Initialize()
    {
      base.Initialize();
    }

    //TODO move this in the drone client
    VideoPacketDecoderWorker videoPacketDecoderWorker;
    DroneClient droneClient;
    OculusClient oculusClient;
    KinectClient kinect;
    RenderTarget2D renderTarget;
    Color[] colorData;
    protected override void LoadContent()
    {
      renderTarget = new RenderTarget2D(GraphicsDevice, graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight);
      spriteBatch = new SpriteBatch(GraphicsDevice);
      hudFontSmall = Content.Load<SpriteFont>(@"Fonts\FontSmall");
      if (drawTestImage)
        videoTexture = Content.Load<Texture2D>(@"test");
      else
        videoTexture = new Texture2D(GraphicsDevice, 640, 360, false, SurfaceFormat.Color);

      colorData = new Color[640 * 360];
      videoPacketDecoderWorker = new VideoPacketDecoderWorker(PixelFormat.BGR24, true, OnVideoPacketDecoded);
      videoPacketDecoderWorker.Start();
      droneClient = new DroneClient();
      droneClient.VideoPacketAcquired += OnVideoPacketAcquired;
      droneClient.ConfigurationUpdated += OnConfigurationUpdated;
      droneClient.Active = true;
      try
      {
        kinect = new KinectClient("it-IT");
        kinect.VoiceCommandRecognized += OnVoiceCommandRecognized;
        kinect.VoiceCommandRejected += OnVoiceCommandRejected;
        kinect.GestureCommandRecognized += OnGestureRecognized;
        kinectMessage = "Kinect trovato. Riconoscimento vocale e comandi attivi...";
      }
      catch (Exception e)
      {
        kinectMessage = e.Message;
      }
      oculusRiftDistortionShader = Content.Load<Effect>("Shaders/OculusRift");
      oculusClient = new OculusClient();
      UpdateResolutionAndRenderTargets();
      screenType = ScreenType.Splash; //The splash show commands
    }

    private void OnConfigurationUpdated(DroneConfiguration configuration)
    {
      if (configuration.Video.Codec != VideoCodecType.H264_360P_SLRS || configuration.Video.MaxBitrate != 100 || configuration.Video.BitrateCtrlMode != VideoBitrateControlMode.Dynamic)
      {
        droneClient.Send(configuration.Video.Codec.Set(VideoCodecType.H264_360P_SLRS).ToCommand());
        droneClient.Send(configuration.Video.MaxBitrate.Set(100).ToCommand());
        droneClient.Send(configuration.Video.BitrateCtrlMode.Set(VideoBitrateControlMode.Dynamic).ToCommand());
      }
    }

    protected override void UnloadContent()
    {
      droneClient.Dispose();
      videoPacketDecoderWorker.Dispose();
      if (kinect != null)
        kinect.Dispose();
    }

    private int viewportWidth;
    private int viewportHeight;
    private Rectangle sideBySideLeftSpriteSize;
    private Rectangle sideBySideRightSpriteSize;
    private void UpdateResolutionAndRenderTargets()
    {
      if (viewportWidth != GraphicsDevice.Viewport.Width || viewportHeight != GraphicsDevice.Viewport.Height)
      {
        viewportWidth = GraphicsDevice.Viewport.Width;
        viewportHeight = GraphicsDevice.Viewport.Height;
        sideBySideLeftSpriteSize = new Rectangle(0, 0, viewportWidth / 2, viewportHeight);
        sideBySideRightSpriteSize = new Rectangle(viewportWidth / 2, 0, viewportWidth / 2, viewportHeight);
      }
    }

    Vector3 headPositionStart = new Vector3();//Value is taken from method TakeOff()
    float xThreshold = 0.2f;
    float yThreshold = 0.2f;
    string oculusYText = string.Empty;
    string oculusXText = string.Empty;
    //head dx, sx, up, down
    private void OculusHandle()
    {
      oculusXText = "On Hold";
      oculusYText = "On Hold";
      var cameraPositionNew = OculusClient.GetOrientation();
      var delta = headPositionStart.X - cameraPositionNew.X;
      var compare = delta;
      if (delta < 0)
        delta *= -1;
      if (delta >= xThreshold)
      {
        if (compare > 0)
        {
          oculusXText = "Right";
          if (lastCommandSent != "right")
          {
            lastCommandSent = "right";
            droneClient.Progress(Drone.Commands.FlightMode.Progressive, yaw: 0.4f);
          }
        }
        else
        {
          oculusXText = "Left";
          if (lastCommandSent != "left")
          {
            lastCommandSent = "left";
            droneClient.Progress(Drone.Commands.FlightMode.Progressive, yaw: -0.4f);
          }
        }
        return;
      }
      delta = headPositionStart.Y - cameraPositionNew.Y;
      compare = delta;
      if (delta < 0)
        delta *= -1;
      if (delta >= yThreshold)
      {
        if (compare > 0)
        {
          oculusYText = "Up";
          if (lastCommandSent != "up")
          {
            lastCommandSent = "up";
            droneClient.Progress(Drone.Commands.FlightMode.Progressive, gaz: 0.4f);
          }
        }
        else
        {
          oculusYText = "Down";
          if (lastCommandSent != "down")
          {
            lastCommandSent = "down";
            droneClient.Progress(Drone.Commands.FlightMode.Progressive, gaz: -0.4f);
          }
        }
      }
    }

    int splashTimeElapsed = 0;
    protected override void Update(GameTime gameTime)
    {
      var keyState = Keyboard.GetState(PlayerIndex.One);
      if (screenType == ScreenType.Splash)
      {
        splashTimeElapsed += gameTime.ElapsedGameTime.Milliseconds;
        if (splashTimeElapsed >= 3000)
          screenType = ScreenType.Drive;
      }
      else
      {
        if (keyState.IsKeyDown(Keys.Enter))
          DroneTakeOff();
        if (keyState.IsKeyDown(Keys.Escape))
          droneClient.Land();
        if (keyState.IsKeyDown(Keys.E))
          droneClient.Emergency();
        UpdateFrame();
      }
      kinectStatusMessage = (kinect != null) ? kinect.StatusMessage : string.Empty;
      base.Update(gameTime);
    }

    private void DroneTakeOff()
    {
      droneClient.FlatTrim();//before take off send this command
      headPositionStart = OculusClient.GetOrientation();
      if (lastCommandSent != "takeoff")
      {
        droneClient.Takeoff();
        lastCommandSent = "takeoff";
      }
    }

    private VideoFrame videoFrame;
    private Texture2D videoTexture;
    private uint videoFrameNumber;
    private void OnVideoPacketDecoded(VideoFrame frame)
    {
      videoFrame = frame;
    }

    private void OnVideoPacketAcquired(VideoPacket packet)
    {
      if (videoPacketDecoderWorker.IsAlive)
        videoPacketDecoderWorker.EnqueuePacket(packet);
    }

    private void UpdateFrame()
    {
      //TODO: move this in the Drone client and expose the frame (remember locking)
      if (videoFrame == null || videoFrameNumber == videoFrame.Number)
        return;
      videoFrameNumber = videoFrame.Number;
      byte[] bgrData = videoFrame.Data;
      for (int i = 0; i < colorData.Length; i++)
        colorData[i] = new Color(bgrData[3 * i + 2], bgrData[3 * i + 1], bgrData[3 * i]);
      if (videoTexture.GraphicsDevice.Textures[0] == videoTexture)
        videoTexture.GraphicsDevice.Textures[0] = null;
      videoTexture.SetData<Color>(0, null, colorData, 0, colorData.Length);
    }

    Texture2D renderTexture;
    protected override void Draw(GameTime gameTime)
    {
      GraphicsDevice.SetRenderTarget(renderTarget);
      GraphicsDevice.Clear(Color.Black);
      spriteBatch.Begin();
      if (screenType == ScreenType.Drive)
      {
        spriteBatch.Draw(videoTexture, new Rectangle(0, 40, 1280, 800), Color.White);//1280X720 su 1280x800
        DrawHud(spriteBatch);
      }
      else
      {
        DrawHelp(spriteBatch);
      }
      spriteBatch.End();

      GraphicsDevice.SetRenderTarget(null);
      renderTexture = (Texture2D)renderTarget;
      GraphicsDevice.Clear(Color.Black);

      if (drawOculus)
      {
        //Set the four Distortion params of the oculus
        oculusRiftDistortionShader.Parameters["distK0"].SetValue(oculusClient.DistK0);
        oculusRiftDistortionShader.Parameters["distK1"].SetValue(oculusClient.DistK1);
        oculusRiftDistortionShader.Parameters["distK2"].SetValue(oculusClient.DistK2);
        oculusRiftDistortionShader.Parameters["distK3"].SetValue(oculusClient.DistK3);
        oculusRiftDistortionShader.Parameters["imageScaleFactor"].SetValue(scaleImageFactor);

        oculusRiftDistortionShader.Parameters["drawLeftLens"].SetValue(true);
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, null, null, null, oculusRiftDistortionShader);
        spriteBatch.Draw(renderTexture, sideBySideLeftSpriteSize, Color.White);
        spriteBatch.End();

        oculusRiftDistortionShader.Parameters["drawLeftLens"].SetValue(false);
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, null, null, null, oculusRiftDistortionShader);
        spriteBatch.Draw(renderTexture, sideBySideRightSpriteSize, Color.White);
        spriteBatch.End();
      }
      else
      {
        spriteBatch.Begin();
        spriteBatch.Draw(renderTexture, new Rectangle(0, 0, Window.ClientBounds.Width, Window.ClientBounds.Height), Color.White);
        spriteBatch.End();
      }
      base.Draw(gameTime);
    }

    private void DrawHelp(SpriteBatch spriteBatch)
    {
      spriteBatch.DrawString(hudFontSmall, String.Format("Voice Commands: Drone decolla, Drone atterra, Cambio camera, Emergenza", oculusXText, oculusYText), new Vector2(400, 220), Color.Blue);
      spriteBatch.DrawString(hudFontSmall, String.Format("Gesture Commands: ", oculusXText, oculusYText), new Vector2(400, 240), Color.Blue);
      spriteBatch.DrawString(hudFontSmall, String.Format("        Left hand or right hand over elbow -> strafe", oculusXText, oculusYText), new Vector2(400, 260), Color.Blue);
      spriteBatch.DrawString(hudFontSmall, String.Format("        Both hands forward or near shoulders -> forward/backward", oculusXText, oculusYText), new Vector2(400, 280), Color.Blue);
    }

    private void DrawHud(SpriteBatch spriteBatch)
    {
      //spriteBatch.DrawString(hudFontSmall, String.Format("Left<->Right: {0}  Top<->Down: {1}", oculusXText, oculusYText), new Vector2(400, 220), Color.White);
      spriteBatch.DrawString(hudFontSmall, String.Format("Last command: {0}", lastCommandSent), new Vector2(400, 240), Color.White);

      //spriteBatch.DrawString(hudFontSmall, String.Format("{0} - {1}", kinectMessage, kinectStatusMessage), new Vector2(400, 560), Color.White);
      spriteBatch.DrawString(hudFontSmall, String.Format("Battery: {0}%  Altitude: {1}", droneClient.NavigationData.Battery.Percentage, droneClient.NavigationData.Altitude), new Vector2(400, 540), Color.White);
    }
  }
}
