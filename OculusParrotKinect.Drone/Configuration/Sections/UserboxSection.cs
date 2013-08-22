using System.Runtime.InteropServices;

namespace OculusParrotKinect.Drone.Configuration.Sections
{
    [StructLayout(LayoutKind.Sequential)]
    public class UserboxSection
    {
        public readonly ReadWriteItem<string> UserboxCmd = new ReadWriteItem<string>("userbox:userbox_cmd");
    }
}