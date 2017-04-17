using System;
using System.IO;
using System.Linq;
using System.Configuration;
using System.Reflection;
using System.Runtime.InteropServices.WindowsRuntime;
using PHttp.Application;
using PHttp.Helpers;

namespace PHttp
{
    public class Startup
    {
        public IPHttpApplication LoadApp(string applicationPath)
        {

            var info = new DirectoryInfo(applicationPath);
            if (!info.Exists) { return null; } //make sure directory exists

            var files = info.GetFiles("*.dll");
            foreach (var file in files) //loop through all dll files in directory
            {
                try
                {
                    var name = AssemblyName.GetAssemblyName(file.FullName);
                    var currentAssembly = Assembly.Load(name);
                    var types = currentAssembly.GetTypes().Where(x => typeof(IPHttpApplication).IsAssignableFrom(x) && !x.IsAbstract && !x.IsInterface).ToList();

                    //Testing
                    //var controllers = currentAssembly.GetTypes().Where(x => typeof(Controller).IsAssignableFrom(x) && !x.IsAbstract && !x.IsInterface).ToList();
                    /*if (controllers.Any())
                    {

                        var controller = (Controller)Activator.CreateInstance(controllers.First());
                        MethodInfo theMethod = controller.GetType().GetMethod("Login");
                      
                       var result =  theMethod.Invoke(controller, new object[] { });
                    }*/

                    foreach (var type in types)
                    {
                        var application = (PHttpApplication)Activator.CreateInstance(type);
                        application.OnPreApplicationStart(type, null);
                        application.Start();
                        return application;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("There was an error. " + ex.Message);
                }
            }

            return null;
        }
    }

}
