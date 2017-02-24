using System;
using PHttp;

namespace Client
{
    class Program
    {
        static void Main(string[] args)
        {
            var pHttpStartup = new Startup();
            pHttpStartup.LoadApps();
            Console.ReadKey();
        }
    }
}
