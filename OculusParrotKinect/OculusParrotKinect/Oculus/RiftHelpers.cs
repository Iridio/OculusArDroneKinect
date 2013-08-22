using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#if WINDOWS
namespace OculusParrotKinect.Oculus
{
  public static class RiftHelpers
  {
    /// <summary>
    /// Returns a view vector containing the view of the OculusRift (needed when using the OculusRift)
    /// </summary>
    /// <param name="viewRotation">Returns the rotation vector defined by the OculusRift headtracking view</param>
    /// <returns></returns>
    public static Vector3 UpdateViewRotation(Vector3 viewRotation)
    {
      Quaternion q = new Quaternion();
      OculusRiftDevice.GetOrientation(ref q);
      Vector3 rot = QuaternionToEuler(q);
      return rot;
    }

    /// <summary>
    /// Creates a view Matrix that is modified to include additional rotation from the OculusRift
    /// </summary>
    /// <param name="eyeLocation"></param>
    /// <param name="forward"></param>
    /// <param name="up"></param>
    /// <returns></returns>
    public static Matrix GetViewMatrixModified(Vector3 eyeLocation, Vector3 forward, Vector3 up)
    {
      Quaternion q = new Quaternion();
      OculusRiftDevice.GetPredictedOrientation(ref q);

      forward = Vector3.Transform(forward, q);
      up = Vector3.Transform(Vector3.Up, q);

      return Matrix.Invert(Matrix.CreateWorld(eyeLocation, forward, up));
    }

    /// <summary>
    /// Converts a Quaternion to Euler
    /// </summary>
    /// <param name="q1"></param>
    /// <returns></returns>
    public static Vector3 QuaternionToEuler(Quaternion q1)
    {
      Vector3 euler = Vector3.UnitX;
      double sqw = q1.W * q1.W;
      double sqx = q1.X * q1.X;
      double sqy = q1.Y * q1.Y;
      double sqz = q1.Z * q1.Z;
      double unit = sqx + sqy + sqz + sqw; // if normalised is one, otherwise is correction factor
      double test = q1.X * q1.Y + q1.Z * q1.W;
      if (test > 0.499 * unit)
      { // singularity at north pole
        euler.X = (float)(2 * Math.Atan2(q1.X, q1.W));
        euler.Y = (float)Math.PI / 2;
        euler.Z = 0;
      }
      else if (test < -0.499 * unit)
      { // singularity at south pole
        euler.X = (float)(-2 * Math.Atan2(q1.X, q1.W));
        euler.Y = (float)-Math.PI / 2;
        euler.Z = 0;
      }
      else
      {
        euler.X = (float)Math.Atan2(2 * q1.Y * q1.W - 2 * q1.X * q1.Z, sqx - sqy - sqz + sqw);
        euler.Y = -(float)Math.Asin(2 * test / unit);
        euler.Z = -(float)Math.Atan2(2 * q1.X * q1.W - 2 * q1.Y * q1.Z, -sqx + sqy - sqz + sqw);
      }
      //adjust due to quaternion weirdness from Rift
      float temp = euler.Y;
      euler.X += (float)Math.PI;
      euler.Y = euler.Z;
      euler.Z = temp;
      return euler;
    }
  }
}
#endif