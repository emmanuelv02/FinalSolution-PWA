using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UrlShortener.DAL.Models
{
    [Table("clicks")]
    public class Click
    {
        [Key]
        [Column("Id")]
        public int Id { get; set; }

        [Column("Referrer")]
        public string Referrer { get; set; }

        [Column("Agent")]
        public string Agent { get; set; }

        [Column("Location")]
        public string Location { get; set; }

        [Column("Platform")]
        public string Platform { get; set; }

        [Column("DateTime")]
        public System.DateTime DateTime { get; set; }

        [Column("UrlId")]
        public int UrlId { get; set; }
    }
}
