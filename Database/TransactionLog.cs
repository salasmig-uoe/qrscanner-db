using SQLite;

namespace QRScanner.Database
{
    [Table("transaction_log")]
    public class TransactionLog
    {
        [PrimaryKey]
        [AutoIncrement]
        [Column("id")]
        public int Id { get; set; }

        [Column("transaction_code")]
        public string TransactionCode { get; set; }

        [Column("item_code")]
        public string ItemCode { get; set; }

        [Column("operation_type")]
        public string OperationType { get; set; }

        [Column("payment_id")]
        public int PaymentId { get; set; }

        [Column("old_quantity")]
        public float OldQuantity { get; set; }

        [Column("old_amount")]
        public float OldAmount { get; set; }

        [Column("old_transaction_type")]
        public string OldTransactionType { get; set; }

        [Column("new_quantity")]
        public float NewQuantity { get; set; }

        [Column("new_amount")]
        public float NewAmount { get; set; }

        [Column("new_transaction_type")]
        public string NewTransactionType { get; set; }

        [Column("created")]
        public DateTime Created { get; set; }
    }
}
