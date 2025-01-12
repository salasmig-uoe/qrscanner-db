using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QRScanner
{
    [SQLite.Table("payment_transaction")]
    public class PaymentTransaction
    {
        [PrimaryKey]
        [AutoIncrement]
        [SQLite.Column("payment_id")]
        public int PaymentId { get; set; }

        [SQLite.Column("item_code")]
        public string ItemCode { get; set; }

        [SQLite.Column("amount")]
        public float Amount { get; set; }

        [SQLite.Column("transaction_type")]
        public String TransactionType { get; set; }

    }
}
