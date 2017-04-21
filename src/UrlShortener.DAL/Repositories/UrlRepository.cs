using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using UrlShortener.DAL.Models;

namespace UrlShortener.DAL.Repositories
{
    public class UrlRepository
    {
        public Url GetByOriginalUrlAndUserId(string originalUrl, int userId)
        {
            using (var context = new UrlShortenerContext())
            {
                var result = context.Urls.FirstOrDefault(x => x.OriginalUrl == originalUrl && x.UserId == userId);
                return result;
            }
        }

        public Url GetNonDeletedByShortenedSuffix(string suffix)
        {
            using (var context = new UrlShortenerContext())
            {
                var result = context.Urls.FirstOrDefault(x => x.ShortenedSuffix == suffix && (!x.IsDeleted.HasValue || !x.IsDeleted.Value));
                return result;
            }
        }

        public Url GetNonDeletedByOriginalUrlAndUsername(string originalUrl, string username)
        {
            using (var context = new UrlShortenerContext())
            {
                var user = context.Users.FirstOrDefault(x => x.Username == username);

                if (user != null)
                {
                    var result = context.Urls.FirstOrDefault(x => x.OriginalUrl == originalUrl && x.UserId == user.Id && (!x.IsDeleted.HasValue || !x.IsDeleted.Value));
                    return result;
                }

                return null;
            }
        }

        public Url GetNonDeletedPublicByOriginalUrl(string originalUrl)
        {
            using (var context = new UrlShortenerContext())
            {
                var result = context.Urls.FirstOrDefault(x => x.OriginalUrl == originalUrl && x.UserId == null && (!x.IsDeleted.HasValue || !x.IsDeleted.Value));
                return result;
            }
        }

        public List<Url> GetNonDeletedByUserId(long userId)
        {
            using (var context = new UrlShortenerContext())
            {
                var result = context.Urls.Where(x => x.UserId == userId && (!x.IsDeleted.HasValue || !x.IsDeleted.Value)).ToList();
                return result;
            }
        }

        public void Save(Url url)
        {
            using (var context = new UrlShortenerContext())
            {
                context.Urls.Add(url);
                context.SaveChanges();
            }
        }

        public void Delete(Url url)
        {
            using (var context = new UrlShortenerContext())
            {
                url.IsDeleted = true;
                context.Entry(url).State = EntityState.Modified;
                context.SaveChanges();
            }
        }

        public void Save(Url url, string username)
        {
            using (var context = new UrlShortenerContext())
            {
                var user = context.Users.FirstOrDefault(x => x.Username == username);

                if (user != null)
                {
                    url.UserId = user.Id;
                    context.Urls.Add(url);
                    context.SaveChanges();
                }
            }
        }
    }
}
