using SQLite;

namespace QRScanner.Database
{
    [Table("art_item")]
    public class ArtItem
    {
        [PrimaryKey]
        [AutoIncrement]
        [Column("id")]
        public int Id { get; set; }

        [Column("title")]
        public string Title { get; set; }

        [Column("work_type")]
        public string Type { get; set; }

        [Column("size")]
        public string Size { get; set; }

        [Column("artist_code")]
        public string ArtistCode { get; set; }

        [Column("price")]
        public float Price { get; set; }

        [Column("amount")]
        public float Amount { get; set; }

        [Column("item_code")]
        public string ItemCode { get; set; }

        [Column("created")]
        public DateTime Created { get; set; }

        [Column("updated")]
        public DateTime Updated { get; set; }
    }
}
