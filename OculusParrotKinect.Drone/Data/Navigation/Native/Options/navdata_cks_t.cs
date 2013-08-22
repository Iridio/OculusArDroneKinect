using System.Runtime.InteropServices;

namespace OculusParrotKinect.Drone.Data.Navigation.Native.Options
{
    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
    public struct navdata_cks_t
    {
        public ushort tag;
        public ushort size;
        public uint cks;
    }
}