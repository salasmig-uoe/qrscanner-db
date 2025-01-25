
using SQLite;


namespace QRScanner.Database
{
    [Table("last_transactions")]
    public class LastTransactions
    {
        [PrimaryKey]
        [AutoIncrement]
        [Column("id")]
        public int Id { get; set; }
        [Column("base_code")]
        public string BaseCode { get; set; }

        [Column("last_folio")]
        public int LastFolio { get; set; }

        [Column("updated")]
        public DateTime Updated { get; set; }
    }
}
