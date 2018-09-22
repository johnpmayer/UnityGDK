using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Improbable.Gdk.BuildSystem.Configuration
{
    [CreateAssetMenu(fileName = "SpatialOS Build Configuration", menuName = CreateMenuPath)]
    public class SpatialOSBuildConfiguration : ScriptableSingleton<SpatialOSBuildConfiguration>
    {
        internal const string CreateMenuPath = "SpatialOS/Build Configuration";

        [SerializeField] private bool isInitialised;

        [SerializeField] public List<WorkerBuildConfiguration> WorkerBuildConfigurations;


        private void Awake()
        {
            if (!isInitialised)
            {
                ResetToDefault();
            }

            if (IsAnAsset())
            {
                UpdateEditorScenesForBuild();
            }
        }
        
        protected bool IsAnAsset()
        {
            var assetPath = AssetDatabase.GetAssetPath(this);

            // If there is an asset path, it is in assets.
            return !string.IsNullOrEmpty(assetPath);
        }

        private void ResetToDefault()
        {
            // Build default settings
            var client = new WorkerBuildConfiguration()
            {
                WorkerPlatform = "UnityClient",
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

            var worker = new WorkerBuildConfiguration()
            {
                WorkerPlatform = "UnityGameLogic",
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

        private void OnValidate()
        {
            if (!isInitialised)
            {
                ResetToDefault();
            }

            if (IsAnAsset())
            {
                UpdateEditorScenesForBuild();
            }
        }

        private SceneAsset[] GetScenesForWorker(string workerPlatform)
        {
            WorkerBuildConfiguration configurationForWorker = null;

            if (WorkerBuildConfigurations != null)
            {
                configurationForWorker =
                    WorkerBuildConfigurations.FirstOrDefault(config => config.WorkerPlatform == workerPlatform);
            }

            return configurationForWorker == null
                ? new SceneAsset[0]
                : configurationForWorker.ScenesForWorker;
        }

        internal void UpdateEditorScenesForBuild()
        {
            EditorApplication.delayCall += () =>
            {
                EditorBuildSettings.scenes =
                    WorkerBuildConfigurations.SelectMany(x => GetScenesForWorker(x.WorkerPlatform))
                        .Select(AssetDatabase.GetAssetPath)
                        .Distinct()
                        .Select(scenePath => new EditorBuildSettingsScene(scenePath, true))
                        .ToArray();
            };
        }

        public BuildEnvironmentConfig GetEnvironmentConfigForWorker(string platform, BuildEnvironment targetEnvironment)
        {
            var config = WorkerBuildConfigurations.FirstOrDefault(x => x.WorkerPlatform == platform);
            if (config == null)
            {
                throw new ArgumentException("Unknown WorkerPlatform " + platform);
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
    }
}
