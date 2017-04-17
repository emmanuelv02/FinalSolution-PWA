using System;
using System.Configuration;
using System.IO;

namespace PHttp.Helpers
{
    public static class RelativePathConfigurationHelper
    {
        public static string ReadConfigurationPath(string configurationName)
        {
            var relativeDir = ConfigurationManager.AppSettings[configurationName];

            if (string.IsNullOrEmpty(relativeDir))
                throw new Exception("The configuration " + configurationName + " could not be loaded");

            var currentDir = Environment.CurrentDirectory;
            var path = currentDir.Substring(0, currentDir.IndexOf(relativeDir.Split('/')[0]));
            path = Path.Combine(path, relativeDir);

            return path;
        }

    }
}
