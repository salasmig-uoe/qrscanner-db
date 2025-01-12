using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace QRScanner
{
    public class LocalDbService
    {
        private const string DB_NAME = "demo_local_db.db3";
        private readonly SQLiteAsyncConnection _connection;

       public LocalDbService()
        {
            String curr_directory = FileSystem.AppDataDirectory;
            _connection = new SQLiteAsyncConnection(Path.Combine(FileSystem.AppDataDirectory,DB_NAME));
            _connection.CreateTableAsync<PaymentTransaction>();
            _connection.CreateTableAsync<ArtItem>();

        }

        public async Task<List<ArtItem>> GetArtItems()
        {
            return await _connection.Table<ArtItem>().ToListAsync();
        }

        public async Task<ArtItem> GetById(int id)
        {
            return await _connection.Table<ArtItem>().Where( x =>x.Id == id).FirstOrDefaultAsync();
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
    }
}
