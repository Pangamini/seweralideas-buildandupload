using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace SeweralIdeas.BuildAndUpload.Editor
{
    /// <summary>The settings of this machine for itch.io: the path to butler, or just 'butler' if it is on the PATH.</summary>
    [Serializable]
    public class ItchMachineSettings
    {
        public string ButlerPath = "butler";
    }

    /// <summary>
    /// Pushes a build to itch.io with butler (https://itch.io/docs/butler/). Log in with "butler login" once, or set
    /// BUTLER_API_KEY in the environment Unity is started from.
    /// </summary>
    [Serializable]
    [MovedFrom(true, "Rockwell.Editor", "Assembly-CSharp-Editor")] // saved by reference in upload profile assets
    public class ItchUploader : BuildUploader<ItchMachineSettings>
    {
        [Tooltip("user/game, as in the itch.io page's URL.")]
        [SerializeField] private string _target = "";

        [Tooltip("Keeps the platform and the purpose apart, e.g. windows-test or web-test. It is created by the first push, and itch takes the platform from its name.")]
        [SerializeField] private string _channel = "";

        public override string Describe() => $"itch.io {_target}:{_channel}";

        public override bool IsConfigured(out string problem)
        {
            if(string.IsNullOrWhiteSpace(_target) || !_target.Contains('/'))
            {
                problem = "Set the itch.io target (user/game).";
                return false;
            }
            if(string.IsNullOrWhiteSpace(_channel))
            {
                problem = "Set the itch.io channel.";
                return false;
            }
            problem = "";
            return true;
        }

        protected override void DrawMachine(ItchMachineSettings machine)
        {
            machine.ButlerPath = EditorGUILayout.TextField(new GUIContent("Butler", "Path to the butler executable, or just 'butler' if it is on the PATH. Kept for this machine only."), machine.ButlerPath);
        }

        public override void Upload(string folder, string version)
        {
            string push = $"\"{Machine.ButlerPath}\" push \"{folder}\" {_target}:{_channel}";
            if(!string.IsNullOrEmpty(version))
                push += $" --userversion \"{version}\"";
            RunInConsole(push);
        }
    }
}
