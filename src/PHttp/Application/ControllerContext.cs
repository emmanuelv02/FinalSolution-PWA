using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PHttp.Application
{
    public class ControllerContext
    {
        public HttpContext HttpContext { get; private set; }
        public string ControllerName { get; private set; }
        public string InvokedActionName { get; private set; }
        public Dictionary<string, string> OtherRouteValue { get; private set; }

        public ControllerContext(HttpContext context, string controllerName, string invokedActionName, Dictionary<string, string> otherRouteValue)
        {
            HttpContext = context;
            ControllerName = controllerName;
            InvokedActionName = invokedActionName;
            OtherRouteValue = otherRouteValue;
        }


    }
}
