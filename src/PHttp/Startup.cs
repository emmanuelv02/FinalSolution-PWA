using System;
using System.IO;
using System.Linq;
using System.Configuration;
using System.Reflection;
using PHttp.Application;

namespace PHttp
{
    public class Startup
    {
        public void LoadApps()
        {
            string path = ConfigurationManager.AppSettings["ApplicationsDir"];

            if (string.IsNullOrEmpty(path)) { return; } //sanity check

            var info = new DirectoryInfo(path);
            if (!info.Exists) { return; } //make sure directory exists

            var files = info.GetFiles("*.dll");
            foreach (var file in files) //loop through all dll files in directory
            {
                try
                {
                    var name = AssemblyName.GetAssemblyName(file.FullName);
                    var currentAssembly = Assembly.Load(name);
                    var types = currentAssembly.GetTypes().Where(x => typeof(IPHttpApplication).IsAssignableFrom(x) && !x.IsAbstract && !x.IsInterface).ToList();

                    foreach (var type in types)
                    {
                        var application = (IPHttpApplication)Activator.CreateInstance(type);
                        application.Start();
                    }

                }
                catch (Exception ex)
                {
                    Console.WriteLine("There was an error. " + ex.Message);
                }
            }
        }
    }

}
