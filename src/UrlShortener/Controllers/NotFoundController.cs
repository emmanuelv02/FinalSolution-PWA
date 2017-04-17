using System;
using System.IO;
using PHttp.Application;
using System.Net;
using Newtonsoft.Json;
using UrlShortener.DAL;
using UrlShortener.DAL.Repositories;

namespace UrlShortener.Controllers
{
    public class NotFoundController : Controller
    {
        public ActionResult Index()
        {
            var expectedUrlSuffix = ControllerContext.HttpContext.Request.Path.Split('/')[2];
            var urlRepository = new UrlRepository();

            var result = urlRepository.GetNonDeletedByShortenedSuffix(expectedUrlSuffix);

            if (result == null)
            {
                ViewBag.Title = "404";
                return View(StatusCode.NotFound);
            }

            //Store Analitics Information

            var click = new click();

            var request = ControllerContext.HttpContext.Request;

            //Datetime
            click.DateTime = DateTime.Now;

            //Referrer
            click.Referrer = request.Headers["Referer"];
            if (string.IsNullOrEmpty(click.Referrer)) click.Referrer = "Unknown";

            //Browser
            var userAgent = request.Headers["user-agent"];
            click.Agent = GetNavigatorName(userAgent);

            //Location
            var location = GetLocation(request.UserHostAddress);
            click.Location = !string.IsNullOrEmpty(location) ? location : "Unknown";

            //Operating system
            var operatingSystem = GetOperatingSystem(userAgent);
            click.Platform = operatingSystem;

            //UrlId
            click.UrlId = result.Id;

            var clickRepository = new ClickRepository();
            clickRepository.Save(click);

            Redirect(result.OriginalUrl);

            return null;
        }

        //BL
        private static string GetNavigatorName(string userAgent)
        {
            string browser;

            if (userAgent.Contains("Chrome/") && !userAgent.Contains("Edge/"))
            {
                browser = "Google Chrome";
            }
            else if (userAgent.Contains(" MSIE") || userAgent.Contains("Trident/"))
            {
                browser = "Internet Explorer";
            }
            else if (userAgent.Contains("Edge/"))
            {
                browser = "Microsoft Edge";
            }
            else if (userAgent.Contains("Gecko/") && userAgent.Contains("Firefox/"))
            {
                browser = "Mozilla Firefox";
            }
            else if (userAgent.Contains("Safari/"))
            {
                browser = "Safari";
            }
            else
            {
                browser = "Unknown";
            }

            return browser;
        }

        // from an answer at http://stackoverflow.com/questions/2904877/user-agent-parsing-using-c-sharp
        private static string GetOperatingSystem(string userAgent)
        {
            string clientOsName;
            if (userAgent.Contains("Windows 98"))
                clientOsName = "Windows 98";
            else if (userAgent.Contains("Windows NT 5.0"))
                clientOsName = "Windows 2000";
            else if (userAgent.Contains("Windows NT 5.1"))
                clientOsName = "Windows XP";
            else if (userAgent.Contains("Windows NT 6.0"))
                clientOsName = "Windows Vista";
            else if (userAgent.Contains("Windows NT 6.1"))
                clientOsName = "Windows 7";
            else if (userAgent.Contains("Windows NT 6.2"))
                clientOsName = "Windows 8";
            else if (userAgent.Contains("Windows NT 10"))
                clientOsName = "Windows 10";
            else if (userAgent.Contains("Windows"))
            {
                clientOsName = GetOsVersion(userAgent, "Windows");
            }
            else if (userAgent.Contains("Android"))
            {
                clientOsName = GetOsVersion(userAgent, "Android");
            }
            else if (userAgent.Contains("Linux"))
            {
                clientOsName = GetOsVersion(userAgent, "Linux");
            }
            else if (userAgent.Contains("iPhone"))
            {
                clientOsName = GetOsVersion(userAgent, "iPhone");
            }
            else if (userAgent.Contains("iPad"))
            {
                clientOsName = GetOsVersion(userAgent, "iPad");
            }
            else if (userAgent.Contains("Macintosh"))
            {
                clientOsName = GetOsVersion(userAgent, "Macintosh");
            }
            else
            {
                clientOsName = "Unknown";
            }

            return clientOsName;
        }

        private static string GetOsVersion(string userAgent, string osName)
        {
            if (userAgent.Split(new[] { osName }, StringSplitOptions.None)[1].Split(new[] { ';', ')' }).Length != 0)
            {
                return string.Format("{0}{1}", osName, userAgent.Split(new[] { osName }, StringSplitOptions.None)[1].Split(new[] { ';', ')' })[0]);
            }

            return osName;
        }

        private string GetLocation(string clientIp)
        {
            var request = WebRequest.Create("http://freegeoip.net/json/" + clientIp);
            request.Method = "GET";
            var response = request.GetResponse();

            using (var dataStream = response.GetResponseStream())
            {
                using (var reader = new StreamReader(dataStream))
                {
                    var responseFromServer = reader.ReadToEnd();
                    var result = JsonConvert.DeserializeObject<dynamic>(responseFromServer);

                    return result.country_name;
                }
            }
        }

    }
}
