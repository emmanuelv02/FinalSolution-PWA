using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UrlShortener.Models
{
    public class UserUrlModel
    {
        public string OriginalUrl { get; set; }
        public string CreationDateString { get; set; }
        public string ShortenedUrl { get; set; }
        public int ClickNumbers { get; set; }
        public string Suffix { get; set; }
    }
}
