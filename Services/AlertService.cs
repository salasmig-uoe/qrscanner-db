using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QRScanner.Services
{
    public class AlertService : IAlertService
    {
        public async Task<bool> ShowConfirmationAsync(string title, string message, string accept, string cancel)
        {
            return await App.Current.MainPage.DisplayAlert(title, message, accept, cancel);
        }

        public async Task ShowMessageAsync(string title, string message, string cancel)
        {
            await App.Current.MainPage.DisplayAlert(title, message, cancel);
        }
    }
}
