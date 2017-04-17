using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace UrlShortener.DAL.Repositories
{
    public class UrlRepository
    {
        public url GetByOriginalUrlAndUserId(string originalUrl, int userId)
        {
            using (var context = new UrlShortenerDbContainer())
            {
                var result = context.urls.FirstOrDefault(x => x.OriginalUrl == originalUrl && x.UserId == userId);
                return result;
            }
        }

        public url GetNonDeletedByShortenedSuffix(string suffix)
        {
            using (var context = new UrlShortenerDbContainer())
            {
                var result = context.urls.FirstOrDefault(x => x.ShortenedSuffix == suffix && (!x.IsDeleted.HasValue || !x.IsDeleted.Value));
                return result;
            }
        }

        public url GetNonDeletedByOriginalUrlAndUsername(string originalUrl, string username)
        {
            using (var context = new UrlShortenerDbContainer())
            {
                var user = context.users.FirstOrDefault(x => x.Username == username);

                if (user != null)
                {
                    var result = context.urls.FirstOrDefault(x => x.OriginalUrl == originalUrl && x.UserId == user.Id && (!x.IsDeleted.HasValue || !x.IsDeleted.Value));
                    return result;
                }

                return null;
            }
        }

        public url GetNonDeletedPublicByOriginalUrl(string originalUrl)
        {
            using (var context = new UrlShortenerDbContainer())
            {
                var result = context.urls.FirstOrDefault(x => x.OriginalUrl == originalUrl && x.UserId == null && (!x.IsDeleted.HasValue || !x.IsDeleted.Value));
                return result;
            }
        }

        public List<url> GetNonDeletedByUserId(long userId)
        {
            using (var context = new UrlShortenerDbContainer())
            {
                var result = context.urls.Where(x => x.UserId == userId && (!x.IsDeleted.HasValue || !x.IsDeleted.Value)).ToList();
                return result;
            }
        }


        public void Save(url url)
        {
            using (var context = new UrlShortenerDbContainer())
            {
                context.urls.Add(url);
                context.SaveChanges();
            }
        }

        public void Delete(url url)
        {
            using (var context = new UrlShortenerDbContainer())
            {
                url.IsDeleted = true;
                context.Entry(url).State = EntityState.Modified;
                context.SaveChanges();
            }
        }

        public void Save(url url, string username)
        {
            using (var context = new UrlShortenerDbContainer())
            {
                var user = context.users.FirstOrDefault(x => x.Username == username);

                if (user != null)
                {
                    url.UserId = user.Id;
                    context.urls.Add(url);
                    context.SaveChanges();
                }
            }
        }
    }
}
