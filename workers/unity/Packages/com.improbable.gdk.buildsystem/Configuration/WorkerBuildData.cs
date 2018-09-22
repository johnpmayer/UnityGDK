using System;
using System.Collections.Generic;
using Improbable.Gdk.BuildSystem.Util;
using UnityEditor;

namespace Improbable.Gdk.BuildSystem.Configuration
{
    public class WorkerBuildData
    {
        public readonly string WorkerPlatform;

        public string PackageName => $"{WorkerPlatform}@{BuildTargetName}";
        
        private string BuildTargetName => BuildTargetNames[buildTarget];

        public string BuildScratchDirectory =>
            PathUtils.Combine(PathUtils.BuildScratchDirectory, PackageName, ExecutableName).ToUnityPath();

        private string ExecutableName => PackageName + BuildPlatformExtensions[buildTarget];
                
        private readonly BuildTarget buildTarget;

        private static readonly Dictionary<BuildTarget, string> BuildTargetNames =
            new Dictionary<BuildTarget, string>
            {
                { BuildTarget.StandaloneWindows, "Windows" },
                { BuildTarget.StandaloneWindows64, "Windows" },
                { BuildTarget.StandaloneLinux64, "Linux" },
                { BuildTarget.StandaloneOSX, "Mac" }
            };

        private static readonly Dictionary<BuildTarget, string> BuildPlatformExtensions =
            new Dictionary<BuildTarget, string>
            {
                { BuildTarget.StandaloneWindows, ".exe" },
                { BuildTarget.StandaloneWindows64, ".exe" },
                { BuildTarget.StandaloneLinux64, "" },
                { BuildTarget.StandaloneOSX, "" }
            };

        public WorkerBuildData(string workerPlatform, BuildTarget buildTarget)
        {
            if (!BuildTargetNames.ContainsKey(buildTarget))
            {
                throw new ArgumentException("Unsupported BuildPlatform " + buildTarget);
            }

            WorkerPlatform = workerPlatform;
            this.buildTarget = buildTarget;
        }
    }
}
