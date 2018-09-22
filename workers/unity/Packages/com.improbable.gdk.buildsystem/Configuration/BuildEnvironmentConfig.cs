using System;
using UnityEditor;
using UnityEngine;

namespace Improbable.Gdk.BuildSystem.Configuration
{
    [Serializable]
    public class BuildEnvironmentConfig
    {
        public SpatialBuildPlatforms BuildPlatforms;
        public BuildOptions BuildOptions = 0;

        [NonSerialized] public bool ShowBuildOptions = false;
        [NonSerialized] public bool ShowBuildPlatforms = false;
        
        internal static SpatialBuildPlatforms GetCurrentBuildPlatform()
        {
            switch (Application.platform)
            {
                case RuntimePlatform.WindowsEditor:
                    return SpatialBuildPlatforms.Windows64;
                case RuntimePlatform.OSXEditor:
                    return SpatialBuildPlatforms.OSX;
                case RuntimePlatform.LinuxEditor:
                    return SpatialBuildPlatforms.Linux;
                default:
                    throw new Exception($"Unsupported platform detected: {Application.platform}");
            }
        }
    }
}
