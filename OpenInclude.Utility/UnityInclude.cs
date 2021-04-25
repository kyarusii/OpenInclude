using System;

namespace OpenInclude.Utility
{
    public class UnityInclude
    {
        private readonly string identityPath;
        private readonly string packageDisplayName = string.Empty;
        private readonly string packageSubDirectory = string.Empty;

        public UnityInclude(string includeLine)
        {
            var removals = new string[] { "#include", " ", "\t", "\"" };
            var path = includeLine;

            foreach (var removal in removals)
            {
                path = path.Replace(removal, "");
            }

            identityPath = path;

            var directoryBlocks = identityPath.Split('/');
            for (int i = 0; i < directoryBlocks.Length; i++)
            {
                if (i == 0) { }
                else if (i == 1)
                {
                    packageDisplayName = directoryBlocks[i];
                }
                else
                {
                    packageSubDirectory += directoryBlocks[i];
                    if (i != directoryBlocks.Length - 1) packageSubDirectory += "/";
                }
            }

            Console.WriteLine(identityPath);
            Console.WriteLine(packageDisplayName);
            Console.WriteLine(packageSubDirectory);

        }

        public string PackageName
        {
            get { return packageDisplayName; }
        }

        public string SubDirectory
        {
            get { return packageSubDirectory; }
        }
    }
}