using System;
using System.IO;

namespace OpenInclude.Utility
{
    public class UnityPath
    {
        private string m_path;

        public string Root { get; }

        public UnityPath(string path)
        {
            this.m_path = path;

            if (path.Contains("Assets"))
            {
                Root = path.Substring(0, path.IndexOf("Assets", StringComparison.Ordinal) - 1);
            }
            else if (path.Contains("Packages"))
            {
                Root = path.Substring(0, path.IndexOf("Packages", StringComparison.Ordinal) - 1);
            }
            else if (path.Contains("Library\\PackageCache"))
            {
                Root = path.Substring(0, path.IndexOf("Library\\PackageCache", StringComparison.Ordinal) - 1);
            }

            Console.WriteLine($"ROOT - {Root}");
        }

        public string GetResolvedPath(string packageName)
        {
            DirectoryInfo di;
            DirectoryInfo[] subDirs;

            di = new DirectoryInfo(Root + "/Packages");
            subDirs = di.GetDirectories();

            foreach (var dir in subDirs)
            {
                if (dir.FullName.Contains(packageName))
                {
                    return dir.FullName;
                }
            }

            di = new DirectoryInfo(Root + "/Library/PackageCache");
            subDirs = di.GetDirectories();

            foreach (var dir in subDirs)
            {
                if (dir.FullName.Contains(packageName))
                {
                    return dir.FullName;
                }
            }

            di = new DirectoryInfo(Root + "/Assets");
            subDirs = di.GetDirectories();

            foreach (var dir in subDirs)
            {
                if (dir.FullName.Contains(packageName))
                {
                    return dir.FullName;
                }
            }

            return string.Empty;
        }
    }
}