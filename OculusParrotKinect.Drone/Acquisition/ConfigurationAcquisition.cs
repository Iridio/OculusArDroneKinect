﻿using System;
using System.Diagnostics;
using System.Net.Sockets;
using System.Threading;
using OculusParrotKinect.Drone.Infrastructure;
using OculusParrotKinect.Drone.Configuration;
using OculusParrotKinect.Drone.Data;

namespace OculusParrotKinect.Drone.Acquisition
{
    public class ConfigurationAcquisition : WorkerBase
    {
        private const int ControlPort = 5559;
        private const int NetworkBufferSize = 0x10000;
        private const int ConfigTimeout = 1000;
        private readonly Action<ConfigurationPacket> _configurationAcquired;
        private readonly NetworkConfiguration _networkConfiguration;

        public ConfigurationAcquisition(NetworkConfiguration networkConfiguration, Action<ConfigurationPacket> configurationAcquired)
        {
            _networkConfiguration = networkConfiguration;
            _configurationAcquired = configurationAcquired;
        }

        protected override void Loop(CancellationToken token)
        {
            using (var tcpClient = new TcpClient(_networkConfiguration.DroneHostname, ControlPort))
            using (NetworkStream stream = tcpClient.GetStream())
            {
                var buffer = new byte[NetworkBufferSize];
                Stopwatch swConfigTimeout = Stopwatch.StartNew();
                while (token.IsCancellationRequested == false && swConfigTimeout.ElapsedMilliseconds < ConfigTimeout)
                {
                    int offset = 0;
                    if (tcpClient.Available == 0)
                    {
                        Thread.Sleep(10);
                    }
                    else
                    {
                        offset += stream.Read(buffer, offset, buffer.Length);

                        // config eof check
                        if (offset > 0 && buffer[offset - 1] == 0x00)
                        {
                            var data = new byte[offset];
                            Array.Copy(buffer, data, offset);
                            var packet = new ConfigurationPacket
                                {
                                    Timestamp = DateTime.UtcNow.Ticks,
                                    Data = data
                                };
                            _configurationAcquired(packet);

                            return;
                        }
                    }
                }
            }
        }
    }
}