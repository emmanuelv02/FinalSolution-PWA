using System;
using System.Collections.Generic;
using System.Diagnostics;
using PHttp;
using System.IO;
using Newtonsoft.Json;
using PHttp.Application;
using PHttp.Helpers;

namespace Client
{
    class Program
    {
        static void Main(string[] args)
        {
            //var result = rc.GetRouteInformation("/Home/Index/4/Hola");

            var pHttpStartup = new Startup();
            //   pHttpStartup.LoadApps();
            //    Console.ReadKey();

            //Load Configuration

            var configurationPath = RelativePathConfigurationHelper.ReadConfigurationPath("ServerConfiguration");

            if (!File.Exists(configurationPath))
            {
                Console.WriteLine("Failed to read the configuration file");
                return;
            }

            var result = JsonConvert.DeserializeObject<ServerConfiguration>(File.ReadAllText(configurationPath));

            var pHttpSites = new Dictionary<string, IPHttpApplication>(StringComparer.OrdinalIgnoreCase);
            foreach (var site in result.Sites)
            {
                Console.WriteLine("Loading app at" + site.PhysicalPath);
                var application = pHttpStartup.LoadApp(site.PhysicalPath);
                if (application != null)
                {
                    pHttpSites.Add(site.VirtualPath, application);
                    Console.WriteLine("Application " + site.VirtualPath + " Loaded");
                }
            }

            using (var server = new HttpServer(result.Port))
            {
                // New requests are signaled through the RequestReceived
                // event.

                server.RequestReceived += (s, e) =>
                {
                    // The response must be written to e.Response.OutputStream.
                    // When writing text, a StreamWriter can be used.
                    if (e.Request.HttpMethod == "GET" && !e.Request.Path.EndsWith("/") && !e.Request.Path.Contains("."))
                    {
                        e.Response.Redirect(e.Request.Path + "/");
                        return;
                    }
                    Console.WriteLine("procesing request from " + e.Request.UserHostName);
                    var siteVirtualPath = UrlHelper.GetSiteVirtualPath(e.Request.Path);
                    if (pHttpSites.ContainsKey(siteVirtualPath))
                    {
                        var requestedSite = pHttpSites[siteVirtualPath];

                        if (requestedSite != null)
                        {
                             requestedSite.ExecuteAction(e.Context);
                        }
                        else
                        {
                            //TODO 
                        }
                    }
                };

                server.Start();

                // Start the default web browser.

          //      Process.Start(String.Format("http://{0}/", server.EndPoint));

                Console.WriteLine("Press any key to continue...");
                Console.ReadKey();

                // When the HttpServer is disposed, all opened connections
                // are automatically closed.
            }
        }
    }
}
