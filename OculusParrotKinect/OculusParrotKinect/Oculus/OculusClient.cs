using Microsoft.Xna.Framework;

namespace OculusParrotKinect.Oculus
{
  public class OculusClient
  {
    public float DistK0;
    public float DistK1;
    public float DistK2;
    public float DistK3;

    public OculusClient()
    {
      OculusRiftDevice.Initialize();
      //Get the 4 distortion coefficients from the oculus
      DistK0 = OculusRiftDevice.DistK0;
      DistK1 = OculusRiftDevice.DistK1;
      DistK2 = OculusRiftDevice.DistK2;
      DistK3 = OculusRiftDevice.DistK3;
    }

    public Vector3 GetDirection()
    {
      Quaternion q = new Quaternion();
      OculusRiftDevice.GetOrientation(ref q);
      return RiftHelpers.QuaternionToEuler(q);
    }

  }
}
