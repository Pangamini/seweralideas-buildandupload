using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace SeweralIdeas.BuildAndUpload.Editor
{
    /// <summary>
    /// Prepares what is uploaded: a copy of the build without the files and folders that must not be shipped (Unity leaves
    /// "DoNotShip" folders next to the player). The upload is made from the copy, so the build itself stays whole.
    /// </summary>
    public static class UploadStaging
    {
        /// <summary>
        /// Replaces the contents of <paramref name="destination"/> with a copy of <paramref name="source"/>, leaving out every
        /// file and folder whose name matches one of <paramref name="excludePatterns"/> (* is any run of characters, the
        /// match is not case-sensitive). Returns the number of entries that were left out.
        /// </summary>
        public static int Stage(string source, string destination, string[] excludePatterns)
        {
            if(Directory.Exists(destination))
                Directory.Delete(destination, true);

            Regex[] excludes = excludePatterns
                .Where(pattern => !string.IsNullOrWhiteSpace(pattern))
                .Select(pattern => new Regex("^" + Regex.Escape(pattern.Trim()).Replace("\\*", ".*") + "$", RegexOptions.IgnoreCase))
                .ToArray();

            int skipped = 0;
            Copy(new DirectoryInfo(source), new DirectoryInfo(destination), excludes, ref skipped);
            return skipped;
        }

        private static void Copy(DirectoryInfo from, DirectoryInfo to, Regex[] excludes, ref int skipped)
        {
            to.Create();

            foreach(FileInfo file in from.GetFiles())
            {
                if(IsExcluded(file.Name, excludes))
                    ++skipped;
                else
                    file.CopyTo(Path.Combine(to.FullName, file.Name));
            }

            foreach(DirectoryInfo directory in from.GetDirectories())
            {
                if(IsExcluded(directory.Name, excludes))
                    ++skipped;
                else
                    Copy(directory, new DirectoryInfo(Path.Combine(to.FullName, directory.Name)), excludes, ref skipped);
            }
        }

        private static bool IsExcluded(string name, Regex[] excludes) => excludes.Any(exclude => exclude.IsMatch(name));
    }
}
