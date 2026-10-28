using System;
using System.ComponentModel;
using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace SeweralIdeas.BuildAndUpload.Editor
{
    /// <summary>
    /// Where a finished build goes: itch.io, Steam, ... Held by an <see cref="UploadProfile"/> (serialized by reference, so its
    /// fields are saved in the profile asset and are shared through the repo). What belongs to a machine, such as the path to a
    /// tool, is not a field but goes in EditorPrefs, see <see cref="BuildUploader{TMachine}"/>.
    /// </summary>
    [Serializable]
    public abstract class BuildUploader
    {
        /// <summary>Where it uploads to, for the log and the window.</summary>
        public abstract string Describe();

        /// <summary>Whether the fields are filled in; if not, <paramref name="problem"/> says what is missing.</summary>
        public abstract bool IsConfigured(out string problem);

        /// <summary>The settings of this machine (tool paths and the like), kept in EditorPrefs.</summary>
        public virtual void DrawMachineSettings() { }

        /// <summary>Starts the upload of the contents of <paramref name="folder"/>. Runs in the background; the result goes to the console.</summary>
        /// <param name="version">The version of the build, or empty if there is none.</param>
        public abstract void Upload(string folder, string version);

        /// <summary>
        /// Runs a command line in a console window of its own: the tool shows its progress there and can ask for a login, and
        /// the Editor stays usable. The window waits for a key when the command is done, so the result can be read.
        /// </summary>
        protected static void RunInConsole(string commandLine)
        {
            Debug.Log($"Uploading in a console window: {commandLine}");
            try
            {
                // The whole command is in quotes for /c, which keeps the quotes inside it.
                Process.Start(new ProcessStartInfo("cmd.exe", $"/c \"{commandLine} & pause\"")
                {
                    UseShellExecute = true,
                    WindowStyle     = ProcessWindowStyle.Normal,
                })?.Dispose();
            }
            catch(Win32Exception e)
            {
                Debug.LogError($"Could not open a console window: {e.Message}");
            }
        }
    }
}
