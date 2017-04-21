using System;
using System.Collections.Generic;
using System.Linq;
using UrlShortener.DAL.Models;

namespace UrlShortener.DAL.Repositories
{
    public class ClickRepository
    {

        public void Save(Click click)
        {
            using (var context = new UrlShortenerContext())
            {
                context.Clicks.Add(click);
                context.SaveChanges();
            }
        }

        public List<Click> GetByUrlId(int urlId)
        {
            using (var context = new UrlShortenerContext())
            {
                var result = context.Clicks.Where(x => x.UrlId == urlId).ToList();
                return result;
            }
        }
        public int GetNumberByUrlId(int urlId)
        {
            using (var context = new UrlShortenerContext())
            {
                var result = context.Clicks.Count(x => x.UrlId == urlId);
                return result;
            }
        }

        public int GetNumberByYear(DateTime date, int urlId)
        {
            using (var context = new UrlShortenerContext())
            {
                var result = context.Clicks.Count(x => x.UrlId == urlId && x.DateTime.Year == date.Year);
                return result;
            }
        }

        public int GetNumberByMonthOfYear(DateTime date, int urlId)
        {
            using (var context = new UrlShortenerContext())
            {
                var result = context.Clicks.Count(x => x.UrlId == urlId && x.DateTime.Year == date.Year && x.DateTime.Month == date.Month);
                return result;
            }
        }

        //Referrers
        public List<string> GetDistinctReferrersByUrlId(int urlId)
        {
            using (var context = new UrlShortenerContext())
            {
                var result = context.Clicks.Where(x => x.UrlId == urlId).Select(x => x.Referrer).Distinct().ToList();
                return result;
            }
        }

        public int GetReferrerClicksCountByUrlId(string referrer, int urlId)
        {
            using (var context = new UrlShortenerContext())
            {
                var result = context.Clicks.Count(x => x.UrlId == urlId && x.Referrer == referrer);
                return result;
            }
        }

        //Browsers

        public List<string> GetDistinctBrowsersByUrlId(int urlId)
        {
            using (var context = new UrlShortenerContext())
            {
                var result = context.Clicks.Where(x => x.UrlId == urlId).Select(x => x.Agent).Distinct().ToList();
                return result;
            }
        }

        public int GetBrowserClicksCountByUrlId(string browser, int urlId)
        {
            using (var context = new UrlShortenerContext())
            {
                var result = context.Clicks.Count(x => x.UrlId == urlId && x.Agent == browser);
                return result;
            }
        }


        //Platform
        public List<string> GetDistinctPlatformsByUrlId(int urlId)
        {
            using (var context = new UrlShortenerContext())
            {
                var result = context.Clicks.Where(x => x.UrlId == urlId).Select(x => x.Platform).Distinct().ToList();
                return result;
            }
        }

        public int GetPlatformClicksCountByUrlId(string platform, int urlId)
        {
            using (var context = new UrlShortenerContext())
            {
                var result = context.Clicks.Count(x => x.UrlId == urlId && x.Platform == platform);
                return result;
            }
        }


        //Location
        public List<string> GetDistinctLocationsByUrlId(int urlId)
        {
            using (var context = new UrlShortenerContext())
            {
                var result = context.Clicks.Where(x => x.UrlId == urlId).Select(x => x.Location).Distinct().ToList();
                return result;
            }
        }

        public int GetLocationClicksCountByUrlId(string location, int urlId)
        {
            using (var context = new UrlShortenerContext())
            {
                var result = context.Clicks.Count(x => x.UrlId == urlId && x.Location == location);
                return result;
            }
        }

    }
}
