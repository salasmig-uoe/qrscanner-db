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
        public string WorkType { get; set; }

        [Column("size")]
        public string Size { get; set; }

        [Column("artist_code")]
        public string ArtistCode { get; set; }

        [Column("artist_name")]
        public string ArtistName { get; set; }

        [Column("price")]
        public float Price { get; set; }

        [Column("amount")]
        public float Amount { get; set; }

        [Column("price_balance")]
        public float PriceBalance { get; set; }

        [Column("amount_balance")]
        public float AmountBalance { get; set; }

        [Column("item_status")]
        public bool ItemStatus { get; set; }

        [Column("item_code")]
        public string ItemCode { get; set; }

        [Column("created")]
        public DateTime Created { get; set; }

        [Column("updated")]
        public DateTime Updated { get; set; }

        [Column("label_type")]
        public string LabelType { get; set; }
    }
}
