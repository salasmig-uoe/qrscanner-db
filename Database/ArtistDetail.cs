using SQLite;

namespace QRScanner.Database
{
    public class ArtistDetail
    {
        [PrimaryKey]
        [AutoIncrement]
        [Column("id")]
        public int Id { get; set; }

        [Column("artist_code")]
        public string ArtistCode { get; set; }

        [Column("artist_name")]
        public string ArtistName { get; set; }

        [Column("artist_email")]
        public string ArtistEmail { get; set; }
    }
}
