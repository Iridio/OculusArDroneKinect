namespace OculusParrotKinect.Drone.Commands
{
    internal class ComWdgCommand : ATCommand
    {
        protected override string ToAt(int sequenceNumber)
        {
            return string.Format("AT*COMWDG={0}\r", sequenceNumber);
        }
    }
}