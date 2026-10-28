using System;
using UnityEditor;

namespace SeweralIdeas.BuildAndUpload.Editor
{
    /// <summary>
    /// An uploader with settings that belong to the machine (tool paths, accounts): <typeparamref name="TMachine"/> is a
    /// serializable class of them, kept by <see cref="MachineSettingsStore{T}"/> and shared by all the uploaders of that type.
    /// </summary>
    [Serializable]
    public abstract class BuildUploader<TMachine> : BuildUploader where TMachine : class, new()
    {
        /// <summary>The settings of this machine.</summary>
        protected static TMachine Machine => MachineSettingsStore<TMachine>.Instance;

        /// <summary>Draws the fields of <paramref name="machine"/>; it is saved if any of them changed.</summary>
        protected abstract void DrawMachine(TMachine machine);

        public sealed override void DrawMachineSettings()
        {
            EditorGUI.BeginChangeCheck();
            DrawMachine(Machine);
            if(EditorGUI.EndChangeCheck())
                MachineSettingsStore<TMachine>.Save();
        }
    }
}
