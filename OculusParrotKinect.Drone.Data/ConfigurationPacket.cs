using System.Runtime.InteropServices;

namespace OculusParrotKinect.Drone.Data
{
    [StructLayout(LayoutKind.Sequential)]
    public struct ConfigurationPacket
    {
        public byte[] Data;
        public long Timestamp;
    }
}