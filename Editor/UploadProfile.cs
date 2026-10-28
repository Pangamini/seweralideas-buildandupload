using System.Linq;
using UnityEditor.Build.Profile;
using UnityEngine;

namespace SeweralIdeas.BuildAndUpload.Editor
{
    /// <summary>
    /// A build and where it goes: the platform and its settings come from a Unity build profile, the destination from an
    /// uploader. Make one per platform and destination (web to itch.io, Windows to itch.io, ...) and pick it in the window.
    /// </summary>
    [CreateAssetMenu(menuName = "SeweralIdeas/Upload Profile", fileName = "UploadProfile")]
    public class UploadProfile : ScriptableObject
    {
        [Tooltip("Platform, scenes and overrides to build with (File > Build Profiles). Empty: the active build target with the scenes enabled in the build settings.")]
        [SerializeField] private BuildProfile _buildProfile;

        [Tooltip("Names of files and folders that are never copied, neither to the upload nor to the archive (* matches any run of characters): what Unity leaves next to the player and nobody needs.")]
        [SerializeField] private string[] _excludeAlways =
        {
            "*_BurstDebugInformation_DoNotShip",
            "*_BackUpThisFolder_ButDontShipItWithYourGame",
        };

        [Tooltip("Names of files and folders that are left out of the upload only, and kept in the archive: the debug symbols, e.g. *.pdb. The upload is made from a copy of the build without them.")]
        [SerializeField] private string[] _excludeFromUpload = { };

        [SerializeReference] private BuildUploader _uploader;

        /// <summary>What the archive leaves out.</summary>
        public string[] ArchiveExcludes => _excludeAlways;

        /// <summary>What the upload leaves out: the same as the archive, and the symbols.</summary>
        public string[] UploadExcludes => _excludeAlways.Concat(_excludeFromUpload).ToArray();

        public BuildProfile  BuildProfile     => _buildProfile;
        public BuildUploader Uploader         => _uploader;
    }
}
