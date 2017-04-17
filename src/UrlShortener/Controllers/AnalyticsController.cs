using PHttp.Application;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UrlShortener.DAL.Repositories;

namespace UrlShortener.Controllers
{
    public class AnalyticsController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.DontShowFooter = true;

            string suffix = null;

            if (ControllerContext.OtherRouteValue.ContainsKey("id"))
            {
                suffix = ControllerContext.OtherRouteValue["id"];
            }

            if (suffix == null) return View("NotFound");

            var urlRepository = new UrlRepository();
            var url = urlRepository.GetNonDeletedByShortenedSuffix(suffix);

            if (url == null) return View("NotFound");

            var siteUrl = GetSiteUrl() + suffix;

            ViewBag.Title = string.Format("Analytics data for {0}", siteUrl);

            var clickRepository = new ClickRepository();
            var totalClicks = clickRepository.GetNumberByUrlId(url.Id);
            ViewBag.TotalClicks = totalClicks;
            //If is older than a year, use another kind of graphic
            if (url.CreationDate >= DateTime.Now.AddMonths(-12))
            {
                ViewBag.ClickGraphTitle = "Monthly Clicks";
                ViewBag.IntervalType = "month";
                ViewBag.ValueFormat = "MMM";

                var dataPoints = new List<ClickDataPoint>();

                var analyticsDate = new DateTime(url.CreationDate.Year, url.CreationDate.Month, 1);

                for (var i = 0; i < 12; i++)
                {
                    var y = clickRepository.GetNumberByMonthOfYear(analyticsDate, url.Id);
                    dataPoints.Add(new ClickDataPoint { X = analyticsDate, Y = y });
                    analyticsDate = analyticsDate.AddMonths(1);
                }


                ViewBag.ClickDataPoints = dataPoints;
            }
            else
            {
                ViewBag.ClickGraphTitle = "Yearly Clics";
                ViewBag.IntervalType = "year";
                ViewBag.ValueFormat = "YYYY";

                var dataPoints = new List<ClickDataPoint>();

                var analyticsDate = new DateTime(url.CreationDate.Year, 1, 1); ;
                var actualYear = DateTime.Today.Year;

                while (analyticsDate.Year <= actualYear)
                {
                    var y = clickRepository.GetNumberByYear(analyticsDate, url.Id);
                    dataPoints.Add(new ClickDataPoint { X = analyticsDate, Y = y });
                    analyticsDate = analyticsDate.AddYears(1);
                }

                ViewBag.ClickDataPoints = dataPoints;
            }


            //Referrers
            var referrers = clickRepository.GetDistinctReferrersByUrlId(url.Id);

            var referrerDataPoints = new List<ReferrerDataPoint>();

            foreach (var referrer in referrers)
            {
                var referrerClicks = clickRepository.GetReferrerClicksCountByUrlId(referrer, url.Id);

                var result = (referrerClicks / (decimal)totalClicks) * 100;
                referrerDataPoints.Add(new ReferrerDataPoint { LegendText = referrer, Y = result });
            }

            ViewBag.ReferrerDataPoints = referrerDataPoints;

            //Browsers
            var browsers = clickRepository.GetDistinctBrowsersByUrlId(url.Id);
            var browserDataPoints = new List<ColumnDataPoirnt>();

            foreach (var browser in browsers)
            {
                var browserClicks = clickRepository.GetBrowserClicksCountByUrlId(browser, url.Id);

                browserDataPoints.Add(new ColumnDataPoirnt { Label = browser, Y = browserClicks });
            }

            ViewBag.BrowserDataPoints = browserDataPoints;

            //Platforms
            var platforms = clickRepository.GetDistinctPlatformsByUrlId(url.Id);
            var platformDataPoints = new List<ColumnDataPoirnt>();

            foreach (var platform in platforms)
            {
                var platformClicks = clickRepository.GetPlatformClicksCountByUrlId(platform, url.Id);

                platformDataPoints.Add(new ColumnDataPoirnt { Label = platform, Y = platformClicks });
            }

            ViewBag.PlatformDataPoints = platformDataPoints;


            //Locations
            var locations = clickRepository.GetDistinctLocationsByUrlId(url.Id);
            var locationDataPoints = new List<ColumnDataPoirnt>();

            foreach (var location in locations)
            {
                var locationClicks = clickRepository.GetLocationClicksCountByUrlId(location, url.Id);

                locationDataPoints.Add(new ColumnDataPoirnt { Label = location, Y = locationClicks });
            }

            ViewBag.LocationDataPoints = locationDataPoints;


            return View();
        }


        public class ColumnDataPoirnt
        {
            public int Y { get; set; }
            public string Label { get; set; }
        }

        public class ReferrerDataPoint
        {
            public decimal Y { get; set; }
            public string LegendText { get; set; }
        }

        public class ClickDataPoint
        {
            public int JsMonth { get { return X.Month - 1; } }
            public DateTime X { get; set; }
            public int Y { get; set; }
        }
    }
}
