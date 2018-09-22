using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Improbable.Gdk.BuildSystem.Configuration;
using Improbable.Gdk.BuildSystem.Util;
using Improbable.Gdk.Tools;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Improbable.Gdk.BuildSystem
{
    public static class WorkerBuilder
    {        
        private const string BuildConfigurationMissingErrorMessage =
            "No objects of type SpatialOSBuildConfiguration found in the project.\nPlease create one using Assets/Create/" +
            EditorConfig.ParentMenu + "/" + EditorConfig.BuildConfigurationMenu + ".";
        
        private static readonly string PlayerBuildDirectory =
            Path.GetFullPath(PathUtils.Combine(Directory.GetCurrentDirectory(), PathUtils.AssetDatabaseDirectory,
                "worker"));

        public static void BuildWorkerForEnvironment(string workerType, BuildEnvironment targetEnvironment)
        {
            var spatialOSBuildConfiguration = SpatialOSBuildConfiguration.instance;
            var environmentConfig = spatialOSBuildConfiguration.GetEnvironmentConfigForWorker(workerType, targetEnvironment);
            var buildPlatforms = environmentConfig.BuildPlatforms;
            var buildOptions = environmentConfig.BuildOptions;
            
            if (!Directory.Exists(PlayerBuildDirectory))
            {
                Directory.CreateDirectory(PlayerBuildDirectory);
            }

            foreach (var unityBuildTarget in GetUnityBuildTargets(buildPlatforms))
            {
                BuildWorkerForTarget(workerType, unityBuildTarget, buildOptions, targetEnvironment);
            }
        }
        
        public static void Clean()
        {
            FileUtil.DeleteFileOrDirectory(PlayerBuildDirectory);
            FileUtil.DeleteFileOrDirectory(PathUtils.BuildScratchDirectory);
        }

        private static IEnumerable<BuildTarget> GetUnityBuildTargets(SpatialBuildPlatforms actualPlatforms)
        {
            var result = new List<BuildTarget>();

            if ((actualPlatforms & SpatialBuildPlatforms.Linux) != 0)
            {
                result.Add(BuildTarget.StandaloneLinux64);
            }

            if ((actualPlatforms & SpatialBuildPlatforms.OSX) != 0)
            {
                result.Add(BuildTarget.StandaloneOSX);
            }

            if ((actualPlatforms & SpatialBuildPlatforms.Windows64) != 0)
            {
                result.Add(BuildTarget.StandaloneWindows64);
            }

            return result.ToArray();
        }

        private static void BuildWorkerForTarget(string workerType, BuildTarget buildTarget,
            BuildOptions buildOptions, BuildEnvironment targetEnvironment)
        {
            var spatialOSBuildConfiguration = SpatialOSBuildConfiguration.instance;

            Debug.LogFormat("Building \"{0}\" for worker platform: \"{1}\", environment: \"{2}\"", buildTarget,
                workerType, targetEnvironment);

            var symbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone);

            try
            {
                var workerBuildData = new WorkerBuildData(workerType, buildTarget);
                var scenes = spatialOSBuildConfiguration.GetScenePathsForWorker(workerType);

                var typeSymbol = $"IMPROBABLE_WORKERTYPE_{workerBuildData.WorkerType}";
                var workerSymbols = symbols.Split(';')
                    .Concat(new[] { typeSymbol })
                    .Distinct()
                    .Aggregate((current, next) => current + ";" + next);
                PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone, workerSymbols);

                var buildPlayerOptions = new BuildPlayerOptions
                {
                    options = buildOptions,
                    target = buildTarget,
                    scenes = scenes,
                    locationPathName = workerBuildData.BuildScratchDirectory
                };

                var result = BuildPipeline.BuildPlayer(buildPlayerOptions);
                if (result.summary.result != BuildResult.Succeeded)
                {
                    throw new BuildFailedException($"Build failed for {workerType}");
                }

                var zipPath = Path.GetFullPath(Path.Combine(PlayerBuildDirectory, workerBuildData.PackageName));

                var basePath = PathUtils.Combine(PathUtils.BuildScratchDirectory, workerBuildData.PackageName);

                Zip(zipPath, basePath, targetEnvironment == BuildEnvironment.Local);
            }
            finally
            {
                PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone, symbols);
            }
        }

        private static void Zip(string zipAbsolutePath, string basePath, bool useCompression)
        {
            var zipFileFullPath = Path.GetFullPath(zipAbsolutePath);

            using (new ShowProgressBarScope($"Package {basePath}"))
            {
                RedirectedProcess.Run(Common.SpatialBinary, "file", "zip", $"--output=\"{zipFileFullPath}\"",
                    $"--basePath=\"{Path.GetFullPath(basePath)}\"", "\"**\"",
                    $"--compression={useCompression}");
            }
        }
    }
}
