using System;
using System.Collections.Generic;
using System.Linq;
using Improbable.Gdk.BuildSystem.Util;
using UnityEditor;
using UnityEngine;

namespace Improbable.Gdk.BuildSystem.Configuration
{
    [CreateAssetMenu(fileName = "SpatialOS Build Configuration", 
        menuName = EditorConfig.ParentMenu + "/" + EditorConfig.BuildConfigurationMenu)]
    public class SpatialOSBuildConfiguration : ScriptableSingleton<SpatialOSBuildConfiguration>
    {
        [SerializeField] private bool isInitialised;
        [SerializeField] public List<WorkerBuildConfiguration> WorkerBuildConfigurations;

        public BuildEnvironmentConfig GetEnvironmentConfigForWorker(string workerType, BuildEnvironment targetEnvironment)
        {
            var config = WorkerBuildConfigurations.FirstOrDefault(x => x.WorkerType == workerType);
            if (config == null)
            {
                throw new ArgumentException($"Unknown worker type {workerType}.");
            }

            return config.GetEnvironmentConfig(targetEnvironment);
        }

        public string[] GetScenePathsForWorker(string workerType)
        {
            return GetScenesForWorker(workerType)
                .Where(sceneAsset => sceneAsset != null)
                .Select(AssetDatabase.GetAssetPath)
                .ToArray();
        }
        
        internal void UpdateEditorScenesForBuild()
        {
            EditorApplication.delayCall += () =>
            {
                EditorBuildSettings.scenes =
                    WorkerBuildConfigurations.SelectMany(x => GetScenesForWorker(x.WorkerType))
                        .Select(AssetDatabase.GetAssetPath)
                        .Distinct()
                        .Select(scenePath => new EditorBuildSettingsScene(scenePath, true))
                        .ToArray();
            };
        }

        private void OnEnable()
        {
            if (!isInitialised)
            {
                ResetToDefault();
            }

            if (!string.IsNullOrEmpty(AssetDatabase.GetAssetPath(this)))
            {
                UpdateEditorScenesForBuild();
            }
        }

        private void ResetToDefault()
        {
            // Build default settings
            var client = new WorkerBuildConfiguration
            {
                WorkerType = "UnityClient",
                ScenesForWorker = AssetDatabase.FindAssets("t:Scene")
                    .Select(AssetDatabase.GUIDToAssetPath)
                    .Where(path => path.Contains("UnityClient"))
                    .Select(AssetDatabase.LoadAssetAtPath<SceneAsset>).ToArray(),
                LocalBuildConfig = new BuildEnvironmentConfig()
                {
                    BuildPlatforms = BuildEnvironmentConfig.GetCurrentBuildPlatform(),
                    BuildOptions = BuildOptions.Development
                },
                CloudBuildConfig = new BuildEnvironmentConfig()
                {
                    BuildPlatforms = BuildEnvironmentConfig.GetCurrentBuildPlatform(),
                }
            };

            var worker = new WorkerBuildConfiguration
            {
                WorkerType = "UnityGameLogic",
                ScenesForWorker = AssetDatabase.FindAssets("t:Scene")
                    .Select(AssetDatabase.GUIDToAssetPath)
                    .Where(path => path.Contains("UnityGameLogic"))
                    .Select(AssetDatabase.LoadAssetAtPath<SceneAsset>).ToArray(),
                LocalBuildConfig = new BuildEnvironmentConfig()
                {
                    BuildPlatforms = BuildEnvironmentConfig.GetCurrentBuildPlatform(),
                    BuildOptions = BuildOptions.EnableHeadlessMode
                },
                CloudBuildConfig = new BuildEnvironmentConfig()
                {
                    BuildPlatforms = SpatialBuildPlatforms.Linux,
                    BuildOptions = BuildOptions.EnableHeadlessMode
                }
            };

            WorkerBuildConfigurations = new List<WorkerBuildConfiguration>
            {
                client,
                worker
            };

            isInitialised = true;
        }

        private SceneAsset[] GetScenesForWorker(string workerType)
        {
            WorkerBuildConfiguration configurationForWorker = null;

            if (WorkerBuildConfigurations != null)
            {
                configurationForWorker =
                    WorkerBuildConfigurations.FirstOrDefault(config => config.WorkerType == workerType);
            }

            return configurationForWorker == null
                ? new SceneAsset[0]
                : configurationForWorker.ScenesForWorker;
        }
    }
}
