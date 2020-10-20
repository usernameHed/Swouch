using System.Collections.Generic;
using System.IO;

namespace unityEssentials.timeScale.extensions
{
    public static class ExtFile
    {
        /// <summary>
        /// Creates a directory at <paramref name="folder"/> if it doesn't exist
        /// </summary>
        /// <param name="folder">true if we created a new directory</param>
        public static bool CreateDirectoryIfNotExist(this string folder)
        {
            if (string.IsNullOrEmpty(folder))
            {
                return (false);
            }

            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
                return (true);
            }
            return (false);
        }
    }
}