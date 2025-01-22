using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QRScanner.Database
{
    [Table("payment_transaction")]
    public class PaymentTransaction
    {
        [PrimaryKey]
        [AutoIncrement]
        [Column("payment_id")]
        public int PaymentId { get; set; }

        [Column("transaction_code")]
        public string TransactionCode { get; set; }

        [Column("item_code")]
        public string ItemCode { get; set; }

        [Column("quantity")]
        public float Quantity { get; set; }

        [Column("amount")]
        public float Amount { get; set; }

        [Column("transaction_type")]
        public string TransactionType { get; set; }

        [Column("created")]
        public DateTime Created { get; set; }

        [Column("updated")]
        public DateTime Updated { get; set; }

    }
}
