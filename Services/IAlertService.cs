using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QRScanner.Services
{
    public interface IAlertService
    {
        Task<bool> ShowConfirmationAsync(string title, string message, string accept, string cancel);
        Task ShowMessageAsync(string title, string message, string cancel);
    }
}
