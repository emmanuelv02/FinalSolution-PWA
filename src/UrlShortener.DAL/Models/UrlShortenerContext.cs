using System.Data.Entity;
using MySql.Data.Entity;

namespace UrlShortener.DAL.Models
{
    [DbConfigurationType(typeof(MySqlEFConfiguration))]
    public class UrlShortenerContext : DbContext
    {
        public UrlShortenerContext() : base("UrlShortenerDbContainer")
        {
        }

        public DbSet<Url> Urls { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Click> Clicks { get; set; }
    }
}

