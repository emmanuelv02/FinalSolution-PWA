using System.Collections.Generic;
using PHttp.Routing;

namespace Mvc
{
    public class RouteConfig
    {
        public static void RegisterRoute(RouteConfiguration route)
        {
            route.MapRoute("Default",
                "{controller}/{action}/{id}",
                 new Dictionary<string, string> { { "controller", "Home" }, { "action", "Index" } });
        }
    }
}
