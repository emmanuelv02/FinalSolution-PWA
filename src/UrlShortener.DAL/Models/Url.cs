using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UrlShortener.DAL.Models
{
    [Table("urls")]
    public class Url
    {
        [Key]
        [Column("Id")]
        public int Id { get; set; }

        [Column("OriginalUrl")]
        public string OriginalUrl { get; set; }

        [Column("ShortenedSuffix")]
        public string ShortenedSuffix { get; set; }

        [Column("UserId")]
        public Nullable<long> UserId { get; set; }

        [Column("CreationDate")]
        public System.DateTime CreationDate { get; set; }

        [Column("IsDeleted")]
        public Nullable<bool> IsDeleted { get; set; }
    }
}
