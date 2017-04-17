using System.Collections.Generic;
using System.Dynamic;
using System.Text;
using System.Text.RegularExpressions;

namespace PHttp.Routing
{
    public class RouteConfiguration
    {
        private  List<Regex> _regexs = new List<Regex>();
        private  readonly List<string> _routeParts = new List<string>();

        private  string _routePattern;

        public  string RouteName { get; private set; }
        public  string RoutePattern
        {
            get
            {
                return _routePattern != null ? _routePattern : string.Empty;
            }
            private set
            {

                _routePattern = value;
            }
        }

        public  Dictionary<string,string> DefaultRoute { get; private set; }

        public  void MapRoute(string routeName, string routePattern, dynamic defaultRoute)
        {
            //Route name
            RouteName = routeName;

            //RoutePattern
            var urlParts = routePattern.Split('/');
            var regexString = new StringBuilder("(/)?");
            foreach (var urlPart in urlParts)
            {
                var cleanedUrlPart = urlPart.Substring(1, urlPart.Length - 2);
                _routeParts.Add(cleanedUrlPart);


                regexString.Append(urlPart != urlParts[urlParts.Length - 1]
                     ? string.Format(@"(?<{0}>[^\s/]*)/", cleanedUrlPart)
                     : string.Format(@"(?<{0}>[^\s/]*)", cleanedUrlPart));
                _regexs.Add(new Regex(regexString.ToString()));
            }


            RoutePattern = routePattern;

            //Default route
            DefaultRoute = defaultRoute;
        }

        public  Dictionary<string, string> GetRouteInformation(string path)
        {
            var result = new Dictionary<string, string>();

            if (!path.EndsWith("/"))
            {
                path = path + "/";
            }

            var regexIndex = path.Split('/').Length - 3;

            if (regexIndex < 0) return result;

            var match = _regexs[regexIndex].Match(path);


            if (match.Success)
            {
                foreach (var routePart in _routeParts)
                {
                    result.Add(routePart, match.Groups[routePart].Success ? match.Groups[routePart].Value : "");
                }
            }

            return result;
        }


    }
}
