using UnityEditor;
using UnityEditor.Build.Reporting;
using System.IO;

namespace MinersWatch.Editor
{
    /// <summary>CI build methods — invoked by GitHub Actions game-ci/unity-builder.</summary>
    public static class CIBuilder
    {
        /// <summary>
        /// Android development debug build.
        /// - Development Build + Script Debugging enabled (managed stack traces on crash!)
        /// - IL2CPP managed stripping DISABLED (all types preserved)
        /// - Full error logging
        /// </summary>
        public static void BuildDebugAndroid()
        {
            // Force IL2CPP + disable stripping (keep ALL managed types)
            PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
            PlayerSettings.SetManagedStrippingLevel(BuildTargetGroup.Android, ManagedStrippingLevel.Disabled);

            var report = BuildPipeline.BuildPlayer(
                GetEnabledScenes(),
                Path.Combine("build", "Android", "MinersWatch-debug.apk"),
                BuildTarget.Android,
                BuildOptions.Development |
                BuildOptions.AllowDebugging |
                BuildOptions.CompressWithLz4 // faster build than LZ4HC
            );

            if (report.summary.result == BuildResult.Failed)
                throw new System.Exception($"Android debug build failed: {report.summary.totalErrors} errors");
        }

        private static string[] GetEnabledScenes()
        {
            var scenes = new string[EditorBuildSettings.scenes.Length];
            for (int i = 0; i < scenes.Length; i++)
                scenes[i] = EditorBuildSettings.scenes[i].path;
            return scenes;
        }
    }
}
