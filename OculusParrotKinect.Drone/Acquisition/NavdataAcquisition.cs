using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using OculusParrotKinect.Drone.Infrastructure;
using OculusParrotKinect.Drone.Configuration;
using OculusParrotKinect.Drone.Data;

namespace OculusParrotKinect.Drone.Acquisition
{
    public class NavdataAcquisition : WorkerBase
    {
        public const int NavdataPort = 5554;
        public const int KeepAliveTimeout = 200;
        public const int NavdataTimeout = 2000;
        private readonly NetworkConfiguration _configuration;
        private readonly Action _onAcquisitionStopped;
        private readonly Action<NavigationPacket> _packetAcquired;
        private bool _isAcquiring;

        public NavdataAcquisition(NetworkConfiguration configuration, Action<NavigationPacket> packetAcquired, Action onAcquisitionStopped)
        {
            _configuration = configuration;
            _packetAcquired = packetAcquired;
            _onAcquisitionStopped = onAcquisitionStopped;
        }

        public bool IsAcquiring
        {
            get { return _isAcquiring; }
        }

        protected override void Loop(CancellationToken token)
        {
            _isAcquiring = false;
            using (var udpClient = new UdpClient(NavdataPort))
                try
                {
                    udpClient.Connect(_configuration.DroneHostname, NavdataPort);

                    SendKeepAliveSignal(udpClient);

                    var remoteEp = new IPEndPoint(IPAddress.Any, NavdataPort);
                    Stopwatch swKeepAlive = Stopwatch.StartNew();
                    Stopwatch swNavdataTimeout = Stopwatch.StartNew();
                    while (token.IsCancellationRequested == false && swNavdataTimeout.ElapsedMilliseconds < NavdataTimeout)
                    {
                        if (udpClient.Available == 0)
                        {
                            Thread.Sleep(1);
                        }
                        else
                        {
                            byte[] data = udpClient.Receive(ref remoteEp);
                            var packet = new NavigationPacket
                                {
                                    Timestamp = DateTime.UtcNow.Ticks,
                                    Data = data
                                };
                            _packetAcquired(packet);

                            _isAcquiring = true;
                            swNavdataTimeout.Restart();
                        }

                        if (swKeepAlive.ElapsedMilliseconds > KeepAliveTimeout)
                        {
                            SendKeepAliveSignal(udpClient);
                            swKeepAlive.Restart();
                        }
                    }
                }
                finally
                {
                    if (_isAcquiring)
                    {
                        _isAcquiring = false;
                        _onAcquisitionStopped();
                    }
                }
        }

        private void SendKeepAliveSignal(UdpClient udpClient)
        {
            byte[] payload = BitConverter.GetBytes(1);
            try
            {
                udpClient.Send(payload, payload.Length);
            }
            catch (SocketException)
            {
            }
        }
    }
}