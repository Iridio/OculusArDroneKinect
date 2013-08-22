﻿using System;
using OculusParrotKinect.Drone.Acquisition.Video.Native;
using OculusParrotKinect.Drone.Data;

namespace OculusParrotKinect.Drone.Acquisition.Video
{
    public static class VideoFrameTypeConverter
    {
        public static VideoFrameType Convert(byte frame_type)
        {
            var frameType = (parrot_video_encapsulation_frametypes_t) frame_type;
            switch (frameType)
            {
                case parrot_video_encapsulation_frametypes_t.FRAME_TYPE_IDR_FRAME:
                case parrot_video_encapsulation_frametypes_t.FRAME_TYPE_I_FRAME:
                    return VideoFrameType.I;
                case parrot_video_encapsulation_frametypes_t.FRAME_TYPE_P_FRAME:
                    return VideoFrameType.P;
                case parrot_video_encapsulation_frametypes_t.FRAME_TYPE_UNKNNOWN:
                case parrot_video_encapsulation_frametypes_t.FRAME_TYPE_HEADERS:
                    return VideoFrameType.Unknown;
                default:
                    throw new ArgumentOutOfRangeException("frame_type");
            }
        }
    }
}