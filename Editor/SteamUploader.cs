using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace SeweralIdeas.BuildAndUpload.Editor
{
    /// <summary>The settings of this machine for Steam: the path to steamcmd (or just 'steamcmd' if it is on the PATH) and the account that uploads.</summary>
    [Serializable]
    public class SteamMachineSettings
    {
        public string SteamCmdPath = "steamcmd";
        public string User = "";
    }

    /// <summary>
    /// Uploads a build to Steam with steamcmd (the Steamworks SDK's ContentBuilder tool). A build script is written next to
    /// the folder and run with "+run_app_build"; the first login asks for the password and the Steam Guard code in the
    /// console window, and steamcmd remembers the login afterwards.
    /// </summary>
    [Serializable]
    [MovedFrom(true, "Rockwell.Editor", "Assembly-CSharp-Editor")] // saved by reference in upload profile assets
    public class SteamUploader : BuildUploader<SteamMachineSettings>
    {
        [SerializeField] private string _appId = "";

        [Tooltip("The depot the files go to: the build's files are the content of this one depot.")]
        [SerializeField] private string _depotId = "";

        [Tooltip("The branch to set the build live on (e.g. 'test'). Empty: the build is only uploaded, and set live in the Steamworks partner site. Steam doesn't allow 'default' to be set from here.")]
        [SerializeField] private string _setLiveBranch = "";

        public override string Describe() => $"Steam app {_appId} depot {_depotId}" + (_setLiveBranch.Length > 0 ? $" -> {_setLiveBranch}" : "");

        public override bool IsConfigured(out string problem)
        {
            if(!IsNumber(_appId) || !IsNumber(_depotId))
            {
                problem = "Set the Steam app id and depot id (numbers).";
                return false;
            }
            if(string.IsNullOrWhiteSpace(Machine.User))
            {
                problem = "Set the Steam account name under 'This machine'.";
                return false;
            }
            problem = "";
            return true;
        }

        protected override void DrawMachine(SteamMachineSettings machine)
        {
            machine.SteamCmdPath = EditorGUILayout.TextField(new GUIContent("steamcmd", "Path to steamcmd.exe, or just 'steamcmd' if it is on the PATH. Kept for this machine only."), machine.SteamCmdPath);
            machine.User         = EditorGUILayout.TextField(new GUIContent("Steam account", "The account that uploads builds; it needs the permission to publish for the app. Kept for this machine only."), machine.User);
        }

        public override void Upload(string folder, string version)
        {
            // The script and steamcmd's log go next to the folder, not into it: whatever is in the folder is uploaded.
            string scriptFolder = Path.Combine(Path.GetDirectoryName(Path.GetFullPath(folder).TrimEnd('\\', '/')), "Steam");
            Directory.CreateDirectory(scriptFolder);
            string scriptPath = Path.Combine(scriptFolder, "app_build.vdf");
            File.WriteAllText(scriptPath, BuildScript(folder, scriptFolder, version));

            RunInConsole($"\"{Machine.SteamCmdPath}\" +login \"{Machine.User}\" +run_app_build \"{scriptPath}\" +quit");
        }

        // The depot is defined in the app script itself, with everything in the folder as its content.
        private string BuildScript(string contentFolder, string outputFolder, string version)
        {
            string description = string.IsNullOrEmpty(version) ? "Unity build" : version;
            string setLive     = _setLiveBranch.Length > 0 ? $"\t\"SetLive\" \"{Escape(_setLiveBranch)}\"\n" : "";

            return
                "\"AppBuild\"\n{\n" +
                $"\t\"AppID\" \"{_appId.Trim()}\"\n" +
                $"\t\"Desc\" \"{Escape(description)}\"\n" +
                setLive +
                $"\t\"ContentRoot\" \"{Escape(Path.GetFullPath(contentFolder))}\"\n" +
                $"\t\"BuildOutput\" \"{Escape(Path.GetFullPath(outputFolder))}\"\n" +
                "\t\"Depots\"\n\t{\n" +
                $"\t\t\"{_depotId.Trim()}\"\n\t\t{{\n" +
                "\t\t\t\"FileMapping\"\n\t\t\t{\n" +
                "\t\t\t\t\"LocalPath\" \"*\"\n" +
                "\t\t\t\t\"DepotPath\" \".\"\n" +
                "\t\t\t\t\"recursive\" \"1\"\n" +
                "\t\t\t}\n\t\t}\n\t}\n}\n";
        }

        // In a Valve script, a backslash and a quote inside a string are escaped with a backslash.
        private static string Escape(string text) => text.Replace("\\", "\\\\").Replace("\"", "\\\"");

        private static bool IsNumber(string text) => ulong.TryParse(text.Trim(), out _);
    }
}
