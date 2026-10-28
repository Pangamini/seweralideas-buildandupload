using System;
using System.Collections.Generic;

namespace SeweralIdeas.BuildAndUpload.Editor
{
    /// <summary>What <see cref="BuildUploadWindow"/> remembers on this machine, kept by <see cref="MachineSettingsStore{T}"/>.</summary>
    [Serializable]
    public class WindowSettings
    {
        [Serializable]
        public class LastBuild
        {
            public string Profile;
            public string Version;
        }

        /// <summary>The chosen upload profile; EditorJsonUtility saves the reference to the asset.</summary>
        public UploadProfile Profile;

        /// <summary>Whether every build is also copied to <see cref="ArchiveFolder"/>.</summary>
        public bool Archive;

        public string ArchiveFolder = "";

        /// <summary>The version typed in for builds uploaded from a chosen folder.</summary>
        public string FolderVersion = "";

        /// <summary>The folder last chosen for an upload.</summary>
        public string LastFolder = "";

        /// <summary>The version of the last build of each profile (by asset name), which is what the build folder holds.</summary>
        public List<LastBuild> LastBuilds = new();

        public string GetLastVersion(string profile)
        {
            LastBuild entry = LastBuilds.Find(build => build.Profile == profile);
            return entry != null ? entry.Version : "";
        }

        public void SetLastVersion(string profile, string version)
        {
            LastBuild entry = LastBuilds.Find(build => build.Profile == profile);
            if(entry == null)
                LastBuilds.Add(new LastBuild { Profile = profile, Version = version });
            else
                entry.Version = version;
        }
    }
}
