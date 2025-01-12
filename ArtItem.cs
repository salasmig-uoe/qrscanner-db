using SQLite;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QRScanner
{
    [SQLite.Table("art_item")]
    public class ArtItem
    {
        [PrimaryKey]
        [AutoIncrement]
        [SQLite.Column("id")]
        public int Id { get; set; }
        
        [SQLite.Column("title")]
        public string Title { get; set; }
        
        [SQLite.Column("work_type")]
        public string Type { get; set; }
        
        [SQLite.Column("size")]
        public string Size { get; set; }
        
        [SQLite.Column("artist_code")]
        public string ArtistCode { get; set; }

        [SQLite.Column("price")]
        public float Price { get; set; }

        [SQLite.Column("amount")]
        public float Amount { get; set; }

        [SQLite.Column("item_code")]
        public string ItemCode { get; set; }
    }
}
