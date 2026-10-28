using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace SeweralIdeas.BuildAndUpload.Editor
{
    /// <summary>
    /// Builds the game by an <see cref="UploadProfile"/> and uploads the build to the destination the profile names.
    /// What the window itself remembers (the chosen profile, the archive folder, ...) belongs to this machine: <see cref="WindowSettings"/>.
    /// </summary>
    public class BuildUploadWindow : EditorWindow
    {
        private UploadProfile _profile;

        private static WindowSettings Settings => MachineSettingsStore<WindowSettings>.Instance;

        [MenuItem("Tools/Build and Upload...")]
        private static void Open() => GetWindow<BuildUploadWindow>("Build and Upload");

        private void OnEnable() => _profile = Settings.Profile;

        private void SetProfile(UploadProfile profile)
        {
            _profile = profile;
            Settings.Profile = profile;
        }

        private void OnGUI()
        {
            EditorGUI.BeginChangeCheck();
            DrawGUI();
            if(EditorGUI.EndChangeCheck())
                MachineSettingsStore<WindowSettings>.Save();
        }

        private void DrawGUI()
        {
            var chosen = (UploadProfile)EditorGUILayout.ObjectField("Upload profile", _profile, typeof(UploadProfile), false);
            if(chosen != _profile)
                SetProfile(chosen);

            if(_profile == null)
            {
                EditorGUILayout.HelpBox("Pick an upload profile, or make one: Assets > Create > SeweralIdeas > Upload Profile.", MessageType.Info);
                return;
            }

            BuildUploader uploader = _profile.Uploader;
            string problem = "No uploader is chosen.";
            bool configured = uploader != null && uploader.IsConfigured(out problem);

            // The profile is edited as an asset, in the inspector.
            using(new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField(uploader != null ? $"Uploads to {uploader.Describe()}" : "No uploader", EditorStyles.miniLabel);
                if(GUILayout.Button("Edit Profile", GUILayout.Width(90)))
                {
                    Selection.activeObject = _profile;
                    EditorGUIUtility.PingObject(_profile);
                }
            }

            EditorGUILayout.Space();
            Settings.Archive = EditorGUILayout.Toggle(new GUIContent("Archive build", "Keeps a copy of every build under a name with the time of the build, with the debug symbols that are left out of the upload. Only the profile's \"Exclude Always\" names are left out."), Settings.Archive);
            if(Settings.Archive)
            {
                using(new EditorGUILayout.HorizontalScope())
                {
                    Settings.ArchiveFolder = EditorGUILayout.TextField(new GUIContent("Archive folder", "Where the archives go. Kept for this machine only."), Settings.ArchiveFolder);
                    if(GUILayout.Button("...", GUILayout.Width(28)))
                    {
                        string picked = EditorUtility.OpenFolderPanel("Archive folder", Directory.Exists(Settings.ArchiveFolder) ? Settings.ArchiveFolder : "", "");
                        if(!string.IsNullOrEmpty(picked))
                            Settings.ArchiveFolder = picked;
                    }
                }
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Build", OutputFolder, EditorStyles.miniLabel);
            EditorGUILayout.LabelField("Upload copy", StageFolder, EditorStyles.miniLabel);
            if(!configured)
                EditorGUILayout.HelpBox(problem, MessageType.Info);

            using(new EditorGUI.DisabledScope(!configured))
            {
                if(GUILayout.Button("Build and Upload"))
                    BuildAndUpload();
            }

            if(GUILayout.Button("Build Only"))
                Build(out _);

            // The build in the profile's folder, as it was left by the last build: its version was saved with it.
            string lastVersion = LastBuildVersion;
            bool hasLastBuild = configured && lastVersion.Length > 0 && Directory.Exists(OutputFolder);
            using(new EditorGUI.DisabledScope(!hasLastBuild))
            {
                if(GUILayout.Button(hasLastBuild ? $"Upload Last Build ({lastVersion})" : "Upload Last Build"))
                    Upload(OutputFolder, lastVersion);
            }

            // Any folder, for a build made some other way: its version isn't known to the tool, so it is typed in (or left out).
            EditorGUILayout.Space();
            Settings.FolderVersion = EditorGUILayout.TextField(new GUIContent("Folder version", "Optional: the version to give the build uploaded from a chosen folder. Left empty, the destination numbers it."), Settings.FolderVersion);
            using(new EditorGUI.DisabledScope(!configured))
            {
                if(GUILayout.Button("Upload Folder..."))
                {
                    string start = Directory.Exists(Settings.LastFolder) ? Settings.LastFolder : Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
                    string folder = EditorUtility.OpenFolderPanel("Folder to upload", start, "");
                    if(!string.IsNullOrEmpty(folder))
                    {
                        Settings.LastFolder = folder;
                        MachineSettingsStore<WindowSettings>.Save();
                        Upload(folder, Settings.FolderVersion.Trim());
                    }
                }
            }
        }

        private string ProfileFolder => Path.GetFullPath(Path.Combine(Application.dataPath, "..", "Builds", _profile.name));

        // What the build makes, and the copy of it that is uploaded.
        private string OutputFolder => Path.Combine(ProfileFolder, "Build");
        private string StageFolder  => Path.Combine(ProfileFolder, "Upload");

        // Uploads a copy of the folder without what must not be shipped, so the folder itself stays whole.
        private void Upload(string folder, string version)
        {
            if(Path.GetFullPath(folder).TrimEnd('\\', '/') == StageFolder.TrimEnd('\\', '/'))
            {
                Debug.LogError("That is the upload copy itself, which is replaced by a new one: pick the build folder.");
                return;
            }

            int skipped;
            try
            {
                skipped = UploadStaging.Stage(folder, StageFolder, _profile.UploadExcludes);
            }
            catch(IOException e)
            {
                Debug.LogError($"Could not stage {folder} in {StageFolder}: {e.Message}. Nothing was uploaded.");
                return;
            }
            catch(System.UnauthorizedAccessException e)
            {
                Debug.LogError($"Could not stage {folder} in {StageFolder}: {e.Message}. Nothing was uploaded.");
                return;
            }

            Debug.Log($"Staged {folder} in {StageFolder}, leaving out {skipped} excluded entries.");
            _profile.Uploader.Upload(StageFolder, version);
        }

        // The version is set during the build and only known then, so it is kept for uploading the build later.
        private string LastBuildVersion
        {
            get => Settings.GetLastVersion(_profile.name);
            set
            {
                Settings.SetLastVersion(_profile.name, value);
                MachineSettingsStore<WindowSettings>.Save();
            }
        }

        private void BuildAndUpload()
        {
            if(!Build(out string outputFolder))
                return;
            Upload(outputFolder, LastBuildVersion);
        }

        private bool Build(out string outputFolder)
        {
            outputFolder = OutputFolder;
            LastBuildVersion = ""; // the folder is about to stop being the build of that version
            BuildVersionCapture.Clear();
            if(Directory.Exists(outputFolder))
                Directory.Delete(outputFolder, true); // no files of an older build left to be uploaded

            string location = outputFolder; // a folder for every platform

            BuildReport report;
            if(_profile.BuildProfile != null)
            {
                // Whether it is a development build is the build profile's own setting.
                report = BuildPipeline.BuildPlayer(new BuildPlayerWithProfileOptions
                {
                    buildProfile     = _profile.BuildProfile,
                    locationPathName = location,
                    options          = BuildOptions.None,
                });
            }
            else
            {
                report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
                {
                    scenes           = EditorBuildSettings.scenes.Where(scene => scene.enabled).Select(scene => scene.path).ToArray(),
                    locationPathName = location,
                    target           = EditorUserBuildSettings.activeBuildTarget,
                    options          = EditorUserBuildSettings.development ? BuildOptions.Development : BuildOptions.None, // as in the build settings
                });
            }

            if(report.summary.result != BuildResult.Succeeded)
            {
                Debug.LogError($"Build {report.summary.result}: {report.summary.totalErrors} errors. Nothing was uploaded.");
                return false;
            }

            // Read by the build process itself: a tool that sets the version for the build only has put the original back by now.
            LastBuildVersion = BuildVersionCapture.Version;
            Debug.Log($"Built {location} ({report.summary.totalSize / (1024 * 1024)} MB), version {LastBuildVersion}.");

            if(Settings.Archive)
                ArchiveBuild(outputFolder, LastBuildVersion);
            return true;
        }

        // A copy of the build with the debug symbols that must not be uploaded, but without the leftovers nobody needs, named by profile, version and time.
        // A failure is logged and doesn't stop the upload: the build itself is fine.
        private void ArchiveBuild(string buildFolder, string version)
        {
            if(string.IsNullOrWhiteSpace(Settings.ArchiveFolder))
            {
                Debug.LogError("Archive build is ticked but there is no archive folder. The build was not archived.");
                return;
            }

            string name = $"{_profile.name}_{(version.Length > 0 ? version + "_" : "")}{System.DateTime.Now:yyyy-MM-dd_HH-mm-ss}";
            foreach(char invalid in Path.GetInvalidFileNameChars())
                name = name.Replace(invalid, '-');

            string destination = Path.Combine(Settings.ArchiveFolder, name);
            try
            {
                UploadStaging.Stage(buildFolder, destination, _profile.ArchiveExcludes);
                Debug.Log($"Archived the build in {destination}.");
            }
            catch(System.Exception e) when(e is IOException || e is System.UnauthorizedAccessException)
            {
                Debug.LogError($"Could not archive the build in {destination}: {e.Message}");
            }
        }
    }
}
