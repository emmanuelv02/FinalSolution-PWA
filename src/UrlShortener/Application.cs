using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PHttp.Application;

namespace UrlShortener
{
    public class Application : PHttpApplication
    {
        public override void Start()
        {
            RouteConfig.RegisterRoute(RouteConfiguration);
        }
    }
}
