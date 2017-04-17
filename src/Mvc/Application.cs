using PHttp.Application;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mvc
{
    public class Application : PHttpApplication
    {
        public override void Start()
        {
            RouteConfig.RegisterRoute(RouteConfiguration);

           // Console.WriteLine("This is a call MVC project start method");
        }
    }
}
