using System;

namespace OculusParrotKinect
{
#if WINDOWS || XBOX
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            using (OculusParrotKinect game = new OculusParrotKinect())
            {
                game.Run();
            }
        }
    }
#endif
}

