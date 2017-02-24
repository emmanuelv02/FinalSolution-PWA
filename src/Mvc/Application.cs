using PHttp.Application;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mvc
{
    public class Application : IPHttpApplication
    {
        public string Name { get; set; }
        public event PreApplicationStartMethod PreApplicationStart;
        public event ApplicationStartMethod ApplicationStart;
        public void Start()
        {
            Console.WriteLine("This is a call MVC project start method");
        }

        public void ExecuteAction()
        {
            Console.WriteLine("This is a call MVC project execute action method");
        }
    }
}
