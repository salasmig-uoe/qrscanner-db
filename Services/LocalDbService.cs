
using QRScanner.Database;
using SQLite;
using System.Globalization;


namespace QRScanner.Services
{
    public class LocalDbService
    {
        private const string DB_NAME = "demo_local_db.db3";
        private readonly SQLiteAsyncConnection _connection;
        
        public LocalDbService()
        {
            
            string curr_directory = FileSystem.AppDataDirectory;
            _connection = new SQLiteAsyncConnection(Path.Combine(FileSystem.AppDataDirectory, DB_NAME));
            
            _connection.CreateTableAsync<ArtItem>().Wait();
            _connection.CreateTableAsync<PaymentTransaction>().Wait();
            _connection.CreateTableAsync<LastTransactions>().Wait();
            
            
        }

        public async Task<List<ArtItem>> GetArtItems()
        {
            return await _connection.Table<ArtItem>().ToListAsync();
        }

        public async Task<ArtItem> GetById(int id)
        {
            return await _connection.Table<ArtItem>().Where(x => x.Id == id).FirstOrDefaultAsync();
        }

        public async Task Create(ArtItem item)
        {
            await _connection.InsertAsync(item);
        }

        public async Task Update(ArtItem item)
        {
            await _connection.UpdateAsync(item);
        }

        public async Task Delete(ArtItem item)
        {
            await _connection.DeleteAsync(item);
        }


        public async Task CreatePaymentItem(PaymentTransaction item)
        {
            await _connection.InsertAsync(item);
        }
        public async Task UpdatePaymentItem(PaymentTransaction item)
        {
            await _connection.UpdateAsync(item);
        }



        public async Task<PaymentTransaction> GetPaymentItemByIdAsync(int id)
        {
            return await _connection.Table<PaymentTransaction>().Where(x => x.PaymentId == id).FirstOrDefaultAsync();
        }

        public async Task<List<PaymentTransaction>> GetDArtItems()
        {
            return await _connection.Table<PaymentTransaction>().ToListAsync();
        }

        public async Task<List<PaymentTransaction>> GetItemsNotDoneAsync(string code, string datestr)
        {

            DateTime dt1 = DateTime.ParseExact(datestr+" 00:00", "yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture);
            DateTime dt2 = DateTime.ParseExact(datestr+" 23:59", "yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture);

            String sql_command = string.Format("" +
                "SELECT * FROM[payment_transaction] " +
                "WHERE[transaction_code] = '{0}' " +
                "and ([created]>={1} and [created]<={2})", code, dt1.Ticks, dt2.Ticks);
            return await _connection.QueryAsync<PaymentTransaction>(sql_command);
        }

        public async Task<List<PaymentTransaction>> GetItemsGroupNotDoneAsync(string itemCode, string groupCode)
        {

            String sql_command = string.Format("" +
                "SELECT * FROM[payment_transaction] " +
                "WHERE[transaction_code] = '{0}' " +
                "and [transaction_code] = {1} ", itemCode, groupCode);
            return await _connection.QueryAsync<PaymentTransaction>(sql_command);
        }


        public async Task<List<PaymentTransaction>> GetPaymentTransferByCodeAndDate(string transaction_code, string date_str)
        {

            DateTime dt1 = DateTime.ParseExact(date_str + " 00:00", "yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture);
            DateTime dt2 = DateTime.ParseExact(date_str + " 23:59", "yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture);

            String sql_command = string.Format("" +
                "SELECT * FROM[payment_transaction] " +
                "WHERE ([created]>={0} and [created]<={1}) " +
                "AND [transaction_code] = \"{2}\" " +
                "ORDER BY [created] ", dt1.Ticks, dt2.Ticks, transaction_code);
            return await _connection.QueryAsync<PaymentTransaction>(sql_command);
        }


        public async Task<List<PaymentTransaction>> GetPaymentTransferByDate( string datestr)
        {

            DateTime dt1 = DateTime.ParseExact(datestr + " 00:00", "yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture);
            DateTime dt2 = DateTime.ParseExact(datestr + " 23:59", "yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture);

            String sql_command = string.Format("" +
                "SELECT * FROM[payment_transaction] " +
                "WHERE ([created]>={0} and [created]<={1}) " +
                "ORDER BY [created] ", dt1.Ticks, dt2.Ticks);
            return await _connection.QueryAsync<PaymentTransaction>(sql_command);
        }


        public async Task<List<PaymentTransaction>> GetDetailItems()
        {
            return await _connection.Table<PaymentTransaction>().ToListAsync();
        }

        public async Task DeletePaymentTransaction(PaymentTransaction item)
        {
            await _connection.DeleteAsync(item);
        }
        //============ Last Transactions ================

        public async Task<LastTransactions> GetLastTransactionAsync(string code)
        {
            return await _connection.Table<LastTransactions>().Where(x => x.BaseCode == code).FirstOrDefaultAsync();
        }

        public async Task CreateLastTransaction(LastTransactions item)
        {
            await _connection.InsertAsync(item);
        }

        public async Task UpdateLastTransaction(LastTransactions item)
        {
            await _connection.UpdateAsync(item);
        }



    }
}
