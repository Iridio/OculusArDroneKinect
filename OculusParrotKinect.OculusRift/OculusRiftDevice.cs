using System;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;
using OculusParrotKinect.OculusRift;

namespace OculusParrotKinect.Oculus
{
  /// <summary>
  /// Interop wrapper class for interacting with Rift
  /// </summary>
  internal class OculusRiftDevice
  {
    // Fields
    private static float AccelGain;
    public static string DisplayDeviceName;
    public static float DistK0;
    public static float DistK1;
    public static float DistK2;
    public static float DistK3;
    private static float DistortionFitScale = 0.7f;
    private static float DistortionFitX;
    private static float DistortionFitY = 1f;
    public static float EyeToScreenDistance;
    public static int HResolution;
    public static float HScreenSize;
    public static float LensSeparationDistance;
    public static float IPD;
    public float InitialAccelGain = 0.05f;
    public float InitialPredictionTime = 0.05f;
    public static float LeftEyeOffset;
    private static float LensOffsetLeft;
    private static float LensOffsetRight;
    private static bool OVRInit;
    private static float PredictionTime;
    public static float RightEyeOffset;
    public static float ScreenVCenter;
    public static int SensorCount;
    public static int VResolution;
    public static float VScreenSize;


    [DllImport("OculusPlugin")]
    private static extern bool OVR_Destroy();

    [DllImport("OculusPlugin", CallingConvention = CallingConvention.Cdecl)]
    private static extern bool OVR_DisplayLatencyScreenColor(ref byte r, ref byte g, ref byte b);

    [DllImport("OculusPlugin", CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr OVR_GetDisplayDeviceName();

    [DllImport("OculusPlugin", CallingConvention = CallingConvention.Cdecl)]
    private static extern bool OVR_GetDistortionCoefficients(ref float k0, ref float k1, ref float k2, ref float k3);

    [DllImport("OculusPlugin", CallingConvention = CallingConvention.Cdecl)]
    private static extern bool OVR_GetEyeOffset(ref float leftEye, ref float rightEye);

    [DllImport("OculusPlugin", CallingConvention = CallingConvention.Cdecl)]
    private static extern bool OVR_GetEyeToScreenDistance(ref float eyeToScreenDistance);

    [DllImport("OculusPlugin")]
    private static extern IntPtr OVR_GetLatencyResultsString();

    [DllImport("OculusPlugin", CallingConvention = CallingConvention.Cdecl)]
    private static extern bool OVR_GetScreenResolution(ref int hResolution, ref int vResolution);

    [DllImport("OculusPlugin", CallingConvention = CallingConvention.Cdecl)]
    private static extern bool OVR_GetScreenSize(ref float hSize, ref float vSize);

    [DllImport("OculusPlugin", CallingConvention = CallingConvention.Cdecl)]
    private static extern bool OVR_GetScreenVCenter(ref float vCenter);

    [DllImport("OculusPlugin", CallingConvention = CallingConvention.Cdecl)]
    private static extern bool OVR_GetSensorAccelGain(int sensorID, ref float accelGain);

    [DllImport("OculusPlugin")]
    private static extern int OVR_GetSensorCount();

    [DllImport("OculusPlugin", CallingConvention = CallingConvention.Cdecl)]
    private static extern bool OVR_GetSensorOrientation(int sensorID, ref float w, ref float x, ref float y, ref float z);

    [DllImport("OculusPlugin", CallingConvention = CallingConvention.Cdecl)]
    private static extern bool OVR_GetSensorPredictedOrientation(int sensorID, ref float w, ref float x, ref float y, ref float z);

    [DllImport("OculusPlugin", CallingConvention = CallingConvention.Cdecl)]
    private static extern bool OVR_GetSensorPredictionTime(int sensorID, ref float predictionTime);

    [DllImport("OculusPlugin", CallingConvention = CallingConvention.Cdecl)]
    private static extern bool OVR_GetInterpupillaryDistance(ref float interpupillaryDistance);

    [DllImport("OculusPlugin", CallingConvention = CallingConvention.Cdecl)]
    private static extern bool OVR_GetLensSeparationDistance(ref float lensSeparationDistance);

    [DllImport("OculusPlugin")]
    private static extern bool OVR_Initialize();

    [DllImport("OculusPlugin")]
    private static extern bool OVR_IsHMDPresent();

    [DllImport("OculusPlugin", CallingConvention = CallingConvention.Cdecl)]
    private static extern bool OVR_IsSensorPresent(int sensor);

    [DllImport("OculusPlugin")]
    private static extern void OVR_ProcessLatencyInputs();

    [DllImport("OculusPlugin")]
    private static extern bool OVR_ResetSensorOrientation(int sensorID);

    [DllImport("OculusPlugin")]
    private static extern bool OVR_SetSensorAccelGain(int sensorID, float accelGain);

    [DllImport("OculusPlugin")]
    private static extern bool OVR_SetSensorPredictionTime(int sensorID, float predictionTime);

    static OculusRiftDevice()
    {
      EmbeddedDllClass.ExtractEmbeddedDlls("OculusPlugin.dll", OculusResource.OculusPlugin);
    }

    public static bool Initialize()
    {
      OVRInit = OVR_Initialize();
      OVRInit |= OVR_GetScreenSize(ref HScreenSize, ref VScreenSize);
      OVRInit |= OVR_GetScreenResolution(ref HResolution, ref VResolution);
      OVRInit |= OVR_GetEyeToScreenDistance(ref EyeToScreenDistance);
      OVRInit |= OVR_GetInterpupillaryDistance(ref IPD);
      OVRInit |= OVR_GetLensSeparationDistance(ref LensSeparationDistance);

      //Get the distorsion coefficients
      OVR_GetDistortionCoefficients(ref DistK0, ref DistK1, ref DistK2, ref DistK3);

      return OVRInit;
    }

    public static IntPtr GetLatencyResultsString()
    {
      return OVR_GetLatencyResultsString();
    }

    public static bool GetOrientation(ref Quaternion q)
    {
      float w = 0f;
      float x = 0f;
      float y = 0f;
      float z = 0f;
      if (OVR_GetSensorOrientation(0, ref w, ref x, ref y, ref z))
      {
        q.W = w;
        q.X = x;
        q.Y = y;
        q.Z = z;
        return true;
      }
      return false;
    }

    public static bool GetPredictedOrientation(ref Quaternion q)
    {
      float w = 0f;
      float x = 0f;
      float y = 0f;
      float z = 0f;
      if (OVR_GetSensorPredictedOrientation(0, ref w, ref x, ref y, ref z))
      {
        q.W = w;
        q.X = x;
        q.Y = y;
        q.Z = z;
        return true;
      }
      return false;
    }

    public static bool IsHMDPresent()
    {
      return OVR_IsHMDPresent();
    }

    public static bool IsInitialized()
    {
      return OVRInit;
    }

    public static bool IsSensorPresent(int sensor)
    {
      return OVR_IsSensorPresent(sensor);
    }

    private static void OnDestroy()
    {
      OVR_Destroy();
      OVRInit = false;
    }
  }
}
