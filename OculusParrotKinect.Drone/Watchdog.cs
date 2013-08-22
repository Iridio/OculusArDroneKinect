using System.Threading;
using OculusParrotKinect.Drone.Acquisition;
using OculusParrotKinect.Drone.Infrastructure;

namespace OculusParrotKinect.Drone
{
    public class Watchdog : WorkerBase
    {
        private readonly CommandSender _commandSender;
        private readonly NavdataAcquisition _navdataAcquisition;
        private readonly VideoAcquisition _videoAcquisition;

        public Watchdog(NavdataAcquisition navdataAcquisition,
                        CommandSender commandSender,
                        VideoAcquisition videoAcquisition)
        {
            _navdataAcquisition = navdataAcquisition;
            _commandSender = commandSender;
            _videoAcquisition = videoAcquisition;
        }

        protected override void Loop(CancellationToken token)
        {
            while (token.IsCancellationRequested == false)
            {
                if (_navdataAcquisition.IsAlive == false)
                {
                    _navdataAcquisition.Start();
                }
                else if (_navdataAcquisition.IsAcquiring)
                {
                    if (_commandSender.IsAlive == false) _commandSender.Start();
                    if (_videoAcquisition.IsAlive == false) _videoAcquisition.Start();
                }
                Thread.Sleep(10);
            }

            _navdataAcquisition.Stop();
            _commandSender.Stop();
            _videoAcquisition.Stop();
        }
    }
}