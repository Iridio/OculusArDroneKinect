using Microsoft.Xna.Framework;
using System;

namespace OculusParrotKinect.Oculus
{
  public class OculusClient
  {
    public float DistK0 { get; private set; }
    public float DistK1 { get; private set; }
    public float DistK2 { get; private set; }
    public float DistK3 { get; private set; }

    public OculusClient()
    {
      OculusRiftDevice.Initialize();
      DistK0 = OculusRiftDevice.DistK0;
      DistK1 = OculusRiftDevice.DistK1;
      DistK2 = OculusRiftDevice.DistK2;
      DistK3 = OculusRiftDevice.DistK3;
    }

    public static Vector3 GetOrientation()
    {
      Quaternion q = new Quaternion();
      OculusRiftDevice.GetOrientation(ref q);
      return q.ToEulerAngles();
    }
  }

  public static class Helpers
  {
    public static Vector3 ToEulerAngles(this Quaternion q)
    {
      Vector3 euler = Vector3.UnitX;
      double sqw = q.W * q.W;
      double sqx = q.X * q.X;
      double sqy = q.Y * q.Y;
      double sqz = q.Z * q.Z;
      double unit = sqx + sqy + sqz + sqw; // if normalised is one, otherwise is correction factor
      double test = q.X * q.Y + q.Z * q.W;
      if (test > 0.499 * unit)
      { // singularity at north pole
        euler.X = (float)(2 * Math.Atan2(q.X, q.W));
        euler.Y = (float)Math.PI / 2;
        euler.Z = 0;
      }
      else if (test < -0.499 * unit)
      { // singularity at south pole
        euler.X = (float)(-2 * Math.Atan2(q.X, q.W));
        euler.Y = (float)-Math.PI / 2;
        euler.Z = 0;
      }
      else
      {
        euler.X = (float)Math.Atan2(2 * q.Y * q.W - 2 * q.X * q.Z, sqx - sqy - sqz + sqw);
        euler.Y = -(float)Math.Asin(2 * test / unit);
        euler.Z = -(float)Math.Atan2(2 * q.X * q.W - 2 * q.Y * q.Z, -sqx + sqy - sqz + sqw);
      }
      //adjust due to quaternion weirdness from Rift
      //TODO check if persists with the new SDK
      float temp = euler.Y;
      euler.X += (float)Math.PI;
      euler.Y = euler.Z;
      euler.Z = temp;
      return euler;
    }
  }
}
