﻿using System;
using System.IO;
using System.Runtime.InteropServices;

namespace OculusParrotKinect.Drone.Infrastructure
{
  public static class InteropHelper
  {
    public const string LD_LIBRARY_PATH = "LD_LIBRARY_PATH";

    public static void RegisterLibrariesSearchPath(string path)
    {
      switch (Environment.OSVersion.Platform)
      {
        case PlatformID.Win32NT:
        case PlatformID.Win32S:
        case PlatformID.Win32Windows:
          SetDllDirectory(path);
          break;
        case PlatformID.Unix:
        case PlatformID.MacOSX:
          string currentValue = Environment.GetEnvironmentVariable(LD_LIBRARY_PATH) ?? string.Empty;
          string newValue = string.IsNullOrEmpty(currentValue) ? path : currentValue + Path.PathSeparator + path;
          Environment.SetEnvironmentVariable(LD_LIBRARY_PATH, newValue);
          break;
      }
    }

    [DllImport("kernel32", SetLastError = true)]
    private static extern bool SetDllDirectory(string lpPathName);
  }
}