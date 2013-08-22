using System.Runtime.InteropServices;

namespace OculusParrotKinect.Drone.Configuration.Sections
{
    [StructLayout(LayoutKind.Sequential)]
    public class LedsSection
    {
        public readonly ReadWriteItem<string> Animation = new ReadWriteItem<string>("leds:leds_anim");
    }
}