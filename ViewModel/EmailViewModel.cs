using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using QRScanner.Model;
using QRScanner.Services;

namespace QRScanner.ViewModel
{
    public partial class EmailViewModel : ObservableObject
    {
        private DateTime _selectedDate;
        private LocalDbService _dbService;
        public Model.Email GetEmail { get; set; }

        public EmailViewModel(LocalDbService dbService)
        {
            _dbService = dbService;
            SelectedDate = DateTime.Today;
            GetEmail = new Model.Email();
        }

        public DateTime SelectedDate
        {
            get => _selectedDate;
            set
            {
                _selectedDate = value;
                OnPropertyChanged(nameof(SelectedDate));
            }
        }

        [RelayCommand]
        async void DetachReport()
        {
            GetEmail.Body = "";
            string message = "All transactions has been removed from report";
            await App.Current.MainPage.DisplayAlert("Report detached: ", message, "Ok");
        }



        [RelayCommand]
        async void AttachReport()
        {
            try
            {
                string formattedDate = _selectedDate.ToString("yyyy-MM-dd");
                var items = await _dbService.GetPaymentTransferByDate(formattedDate);
                string report_content = "\n\n";
                int num_items = items.Count;

                foreach (var item in items)
                {
                    report_content += item.PaymentId + ";" + item.TransactionCode + ";" + item.ItemCode + ";" + item.Quantity + ";" + item.Amount + ";" + item.TransactionType + ";" + item.Created + ";" + item.Updated + "\n";
                }
                if (report_content != "")
                {
                    GetEmail.Body += report_content;
                }
                string message = num_items.ToString() + " transactions included for the " + _selectedDate.ToString("dd/MM/yyyy");
                await App.Current.MainPage.DisplayAlert("Report Attached ", message, "Ok");
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error", ex.Message, "Ok");
            }
        }
        [RelayCommand]
        async void SendMail()
        {
            try
            {
                if (string.IsNullOrEmpty(GetEmail.Subject) ||
                   string.IsNullOrEmpty(GetEmail.Body) ||
                   string.IsNullOrEmpty(GetEmail.TO))
                {
                    await Shell.Current.DisplayAlert("Error", "Please fill in the required fields", "Ok");
                    return;
                }

                var message = new EmailMessage()
                {
                    Subject = GetEmail.Subject,
                    Body = GetEmail.Body,
                    To = new List<string>(GetEmail.TO.Split(';'))
                };

                if (GetEmail.CC.Length > 0)
                    message.Cc = new List<string>(GetEmail.CC.Split(';'));

                await Microsoft.Maui.ApplicationModel.Communication.Email.Default.ComposeAsync(message);
            }
            catch (FeatureNotSupportedException fbsEx)
            {
                await Shell.Current.DisplayAlert("Error", fbsEx.Message, "Ok");
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error", ex.Message, "Ok");
            }
        }
    }
}