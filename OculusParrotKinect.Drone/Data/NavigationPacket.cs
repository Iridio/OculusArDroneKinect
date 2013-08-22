using System.Runtime.InteropServices;

namespace OculusParrotKinect.Drone.Data
{
    [StructLayout(LayoutKind.Sequential)]
    public struct NavigationPacket
    {
        public long Timestamp;
        public byte[] Data;
    }
}