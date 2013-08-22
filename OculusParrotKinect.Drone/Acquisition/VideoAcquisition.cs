﻿using System;
using System.Net.Sockets;
using System.Threading;
using OculusParrotKinect.Drone.Data;
using OculusParrotKinect.Drone.Acquisition.Video;
using OculusParrotKinect.Drone.Acquisition.Video.Native;
using OculusParrotKinect.Drone.Infrastructure;

namespace OculusParrotKinect.Drone.Acquisition
{
  public class VideoAcquisition : WorkerBase
  {
    public const int VideoPort = 5555;
    public const int FrameBufferSize = 0x100000;
    public const int NetworkStreamReadSize = 0x1000;
    private readonly NetworkConfiguration _configuration;
    private readonly Action<VideoPacket> _videoPacketAcquired;

    public VideoAcquisition(NetworkConfiguration configuration, Action<VideoPacket> videoPacketAcquired)
    {
      _configuration = configuration;
      _videoPacketAcquired = videoPacketAcquired;
    }

    protected override unsafe void Loop(CancellationToken token)
    {
      using (var tcpClient = new TcpClient(_configuration.DroneHostname, VideoPort))
      using (NetworkStream stream = tcpClient.GetStream())
      {
        var packet = new VideoPacket();
        byte[] packetData = null;
        int offset = 0;
        int frameStart = 0;
        int frameEnd = 0;
        var buffer = new byte[FrameBufferSize];
        fixed (byte* pBuffer = &buffer[0])
          while (token.IsCancellationRequested == false)
          {
            int read = stream.Read(buffer, offset, NetworkStreamReadSize);

            if (read == 0)
            {
              Thread.Sleep(1);
              continue;
            }

            offset += read;
            if (packetData == null)
            {
              // lookup for a frame start
              int maxSearchIndex = offset - sizeof(parrot_video_encapsulation_t);
              for (int i = 0; i < maxSearchIndex; i++)
              {
                if (buffer[i] == 'P' && buffer[i + 1] == 'a' && buffer[i + 2] == 'V' && buffer[i + 3] == 'E')
                {
                  parrot_video_encapsulation_t pve = *(parrot_video_encapsulation_t*)(pBuffer + i);
                  packetData = new byte[pve.payload_size];
                  packet = new VideoPacket
                  {
                    Timestamp = DateTime.UtcNow.Ticks,
                    FrameNumber = pve.frame_number,
                    Width = pve.display_width,
                    Height = pve.display_height,
                    FrameType = VideoFrameTypeConverter.Convert(pve.frame_type),
                    Data = packetData
                  };
                  frameStart = i + pve.header_size;
                  frameEnd = frameStart + packet.Data.Length;
                  break;
                }
              }
              if (packetData == null)
              {
                // frame is not detected
                offset -= maxSearchIndex;
                Array.Copy(buffer, maxSearchIndex, buffer, 0, offset);
              }
            }
            if (packetData != null && offset >= frameEnd)
            {
              // frame acquired
              Array.Copy(buffer, frameStart, packetData, 0, packetData.Length);
              _videoPacketAcquired(packet);

              // clean up acquired frame
              packetData = null;
              offset -= frameEnd;
              Array.Copy(buffer, frameEnd, buffer, 0, offset);
            }
          }
      }
    }
  }
}