using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

namespace SeweralIdeas.BuildAndUpload.Editor
{
    /// <summary>
    /// Reads the version of the build while it is being made. Tools that set the version for a build only (such as one that
    /// injects it from git and puts the original back afterwards) leave nothing to read once the build is done, so it is read
    /// here, in the build process, after those have run.
    /// </summary>
    public class BuildVersionCapture : IPreprocessBuildWithReport
    {
        /// <summary>The version of the build that was made last, or empty if none was started since <see cref="Clear"/>.</summary>
        public static string Version { get; private set; } = "";

        // After the processors that set the version (they have the default order, 0).
        public int callbackOrder => 1000;

        public static void Clear() => Version = "";

        public void OnPreprocessBuild(BuildReport report) => Version = PlayerSettings.bundleVersion;
    }
}
