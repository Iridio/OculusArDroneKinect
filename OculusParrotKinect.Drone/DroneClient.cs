﻿using System;
using System.Collections.Concurrent;
using OculusParrotKinect.Drone.Acquisition;
using OculusParrotKinect.Drone.Data;
using OculusParrotKinect.Drone.Data.Navigation;
using OculusParrotKinect.Drone.Infrastructure;
using OculusParrotKinect.Drone.Commands;
using OculusParrotKinect.Drone.Configuration;

namespace OculusParrotKinect.Drone
{
    public class DroneClient : DisposableBase
    {
        private static readonly string DefaultHostname = "192.168.1.1";
        private readonly ConcurrentQueue<ATCommand> _commandQueue;
        private readonly CommandSender _commandSender;
        private readonly DroneConfiguration _droneConfiguration;
        private readonly NetworkConfiguration _networkConfiguration;
        private readonly ConfigurationAcquisition _configurationAcquisition;
        private readonly NavdataAcquisition _navdataAcquisition;
        private readonly VideoAcquisition _videoAcquisition;
        private readonly Watchdog _watchdog;
        private bool _initializationRequested;
        private NavigationData _navigationData;
        private StateRequest _stateRequest;

        public DroneClient(string hostname)
        {
            _networkConfiguration = new NetworkConfiguration(hostname);
            _droneConfiguration = new DroneConfiguration();

            _commandQueue = new ConcurrentQueue<ATCommand>();

            _commandSender = new CommandSender(_networkConfiguration, _commandQueue);
            _navdataAcquisition = new NavdataAcquisition(_networkConfiguration, OnNavdataPacketAcquired, OnNavdataAcquisitionStopped);
            _videoAcquisition = new VideoAcquisition(_networkConfiguration, OnVideoPacketAcquired);
            _configurationAcquisition = new ConfigurationAcquisition(_networkConfiguration, OnConfigurationPacketAcquired);
            _watchdog = new Watchdog(_navdataAcquisition, _commandSender, _videoAcquisition);
        }

        public DroneClient() : this (DefaultHostname)
        {
        }

        public Action<NavigationPacket> NavigationPacketAcquired { get; set; }

        public Action<NavigationData> NavigationDataUpdated { get; set; }

        public Action<VideoPacket> VideoPacketAcquired { get; set; }

        public Action<ConfigurationPacket> ConfigurationPacketAcquired { get; set; }

        public Action<DroneConfiguration> ConfigurationUpdated { get; set; }

        public bool Active
        {
            get { return _watchdog.IsAlive; }
            set
            {
                if (value)
                    _watchdog.Start();
                else
                {
                    _watchdog.Stop();
                }
            }
        }

        public bool IsConnected
        {
            get { return _navdataAcquisition.IsAcquiring; }
        }

        public DroneConfiguration Configuration
        {
            get { return _droneConfiguration; }
        }

        public NavigationData NavigationData
        {
            get { return _navigationData; }
        }

        public void Send(ATCommand command)
        {
            _commandQueue.Enqueue(command);
        }

        private void OnNavdataAcquisitionStopped()
        {
            _initializationRequested = false;
            _videoAcquisition.Stop();
        }

        private void OnNavdataPacketAcquired(NavigationPacket packet)
        {
            if (NavigationPacketAcquired != null)
                NavigationPacketAcquired(packet);

            UpdateNavigationData(packet);
        }

        private void UpdateNavigationData(NavigationPacket packet)
        {
            NavigationData navigationData;
            if (NavigationPacketParser.TryParse(ref packet, out navigationData))
            {
                _navigationData = navigationData;


                ProcessTransition();

                if (NavigationDataUpdated != null)
                    NavigationDataUpdated(_navigationData);
            }
        }

        private void OnVideoPacketAcquired(VideoPacket packet)
        {
            if (VideoPacketAcquired != null)
                VideoPacketAcquired(packet);
        }

        private void OnConfigurationPacketAcquired(ConfigurationPacket packet)
        {
            if (ConfigurationPacketAcquired != null)
                ConfigurationPacketAcquired(packet);

            if (ConfigurationPacketParser.TryUpdate(_droneConfiguration, packet))
            {
                if (ConfigurationUpdated != null)
                    ConfigurationUpdated(_droneConfiguration);
            }
        }

        private void ProcessTransition()
        {
            if (_initializationRequested == false)
            {
                _initializationRequested = true;
                _stateRequest = StateRequest.Initialization;
            }

            switch (_stateRequest)
            {
                case StateRequest.None:
                    return;
                case StateRequest.Initialization:
                    _commandQueue.Flush();
                    ATCommand cmdNavdataDemo = _droneConfiguration.General.NavdataDemo.Set(false).ToCommand();
                    Send(cmdNavdataDemo);
                    Send(new ControlCommand(ControlMode.NoControlMode));
                    _stateRequest = StateRequest.Configuration;
                    return;
                case StateRequest.Configuration:
                    _configurationAcquisition.Start();
                    if (_navigationData.State.HasFlag(NavigationState.Command))
                    {
                        Send(new ControlCommand(ControlMode.AckControlMode));
                    }
                    else
                    {
                        Send(new ControlCommand(ControlMode.CfgGetControlMode));
                        _stateRequest = StateRequest.None;
                    }
                    break;
                case StateRequest.Emergency:
                    if (_navigationData.State.HasFlag(NavigationState.Flying))
                        Send(new RefCommand(RefMode.Emergency));
                    else
                        _stateRequest = StateRequest.None;
                    break;
                case StateRequest.ResetEmergency:
                    Send(new RefCommand(RefMode.Emergency));
                    _stateRequest = StateRequest.None;
                    break;
                case StateRequest.Land:
                    if (_navigationData.State.HasFlag(NavigationState.Flying) &&
                        _navigationData.State.HasFlag(NavigationState.Landing) == false)
                    {
                        Send(new RefCommand(RefMode.Land));
                    }
                    else
                        _stateRequest = StateRequest.None;
                    break;
                case StateRequest.Fly:
                    if (_navigationData.State.HasFlag(NavigationState.Landed) &&
                        _navigationData.State.HasFlag(NavigationState.Takeoff) == false &&
                        _navigationData.State.HasFlag(NavigationState.Emergency) == false &&
                        _navigationData.Battery.Low == false)
                    {
                        Send(new RefCommand(RefMode.Takeoff));
                    }
                    else
                        _stateRequest = StateRequest.None;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void Emergency()
        {
            _stateRequest = StateRequest.Emergency;
        }

        public void ResetEmergency()
        {
            _stateRequest = StateRequest.ResetEmergency;
        }

        public void RequestConfiguration()
        {
            _stateRequest = StateRequest.Configuration;
        }

        public void Land()
        {
            _stateRequest = StateRequest.Land;
        }

        public void Takeoff()
        {
            if (_navigationData.State.HasFlag(NavigationState.Landed))
                _stateRequest = StateRequest.Fly;
        }

        public void FlatTrim()
        {
            if (_navigationData.State.HasFlag(NavigationState.Landed))
                Send(new FlatTrimCommand());
        }

        public void Hover()
        {
            if (_navigationData.State.HasFlag(NavigationState.Flying))
                Send(new ProgressCommand(FlightMode.Hover, 0, 0, 0, 0));
        }

        /// <summary>
        /// This command controls the drone flight motions.
        /// </summary>
        /// <param name="mode">Enabling the use of progressive commands and/or the Combined Yaw mode (bitfield).</param>
        /// <param name="roll">Drone left-right tilt - value in range [−1..1].</param>
        /// <param name="pitch">Drone front-back tilt - value in range [−1..1].</param>
        /// <param name="yaw">Drone angular speed - value in range [−1..1].</param>
        /// <param name="gaz">Drone vertical speed - value in range [−1..1].</param>
        public void Progress(FlightMode mode, float roll = 0, float pitch = 0, float yaw = 0, float gaz = 0)
        {
            if (roll > 1 || roll < -1)
                throw new ArgumentOutOfRangeException("roll");
            if (pitch > 1 || pitch < -1)
                throw new ArgumentOutOfRangeException("pitch");
            if (yaw > 1 || yaw < -1)
                throw new ArgumentOutOfRangeException("yaw");
            if (gaz > 1 || gaz < -1)
                throw new ArgumentOutOfRangeException("gaz");

            if (_navigationData.State.HasFlag(NavigationState.Flying))
                Send(new ProgressCommand(mode, roll, pitch, yaw, gaz));
        }

        /// <summary>
        /// This command controls the drone flight motions.
        /// </summary>
        /// <param name="mode">Enabling the use of progressive commands and/or the Combined Yaw mode (bitfield).</param>
        /// <param name="roll">Drone left-right tilt - value in range [−1..1].</param>
        /// <param name="pitch">Drone front-back tilt - value in range [−1..1].</param>
        /// <param name="yaw">Drone angular speed - value in range [−1..1].</param>
        /// <param name="gaz">Drone vertical speed - value in range [−1..1].</param>
        /// <param name="psi">Magneto psi - value in range [−1..1]</param>
        /// <param name="accuracy">Magneto psi accuracy - value in range [−1..1].</param>
        public void ProgressWithMagneto(FlightMode mode, float roll = 0, float pitch = 0, float yaw = 0, float gaz = 0, float psi = 0, float accuracy = 0)
        {
            if (roll > 1 || roll < -1)
                throw new ArgumentOutOfRangeException("roll");
            if (pitch > 1 || pitch < -1)
                throw new ArgumentOutOfRangeException("pitch");
            if (yaw > 1 || yaw < -1)
                throw new ArgumentOutOfRangeException("yaw");
            if (gaz > 1 || gaz < -1)
                throw new ArgumentOutOfRangeException("gaz");
            if (psi > 1 || psi < -1)
                throw new ArgumentOutOfRangeException("psi");
            if (accuracy > 1 || accuracy < -1)
                throw new ArgumentOutOfRangeException("accuracy");

            if (_navigationData.State.HasFlag(NavigationState.Flying))
                Send(new ProgressWithMagnetoCommand(mode, roll, pitch, yaw, gaz, psi, accuracy));
        }

        protected override void DisposeOverride()
        {
            _configurationAcquisition.Dispose();
            _navdataAcquisition.Dispose();
            _commandSender.Dispose();
            _videoAcquisition.Dispose();
            _watchdog.Dispose();
        }
    }
}