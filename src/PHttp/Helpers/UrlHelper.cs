using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PHttp.Helpers
{
    public static class UrlHelper
    {
        public static string GetSiteVirtualPath(string urlPath)
        {
            var result = urlPath.Split('/');
            if (result.Length > 1)
            {
                return "/"+result[1];
            }

            return null;
        }
    }
}
