using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client
{
    internal class ServerConfiguration
    {
        public int Port { get; set; }
        public Dictionary<string, string> ErrorPages { get; set; }
        public List<string> DefaultDocuments { get; set; }
        public List<Site> Sites { get; set; }
    }


    internal class Site
    {
        public string Name { get; set; }
        public string VirtualPath { get; set; }
        public string PhysicalPath { get; set; }
        public bool DirectoryBrowsing { get; set; }
        public Dictionary<string, string> ErrorPages { get; set; }
        public List<string> DefaultDocuments { get; set; }

    }
}