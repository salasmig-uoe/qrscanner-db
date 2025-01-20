using QRScanner.Database;
using QRScanner.Services;
using QRScanner.ViewModel;
namespace QRScanner.Pages;

public partial class DetailViewPage : ContentPage
{
    private readonly LocalDbService _dbService;
    private int _editPaymentTransferRecordId;
    private DateTime _editCreateUpdateDate;
    private int _editPaymentItemId;
    private int _capturedItem;


    public DetailViewPage(LocalDbService dbService, MainViewModel vm)
	{
		InitializeComponent();
        BindingContext = vm;
        _dbService = dbService;
        Task.Run(async () =>listView.ItemsSource = await _dbService.GetItemsNotDoneAsync("c0", "Card", "2025-01-19"));
    }

    // Method to update the Picker based on a string value
    public void UpdateTransactionTypePicker(string newValue)
    {
        if (transactionTypePicker.ItemsSource is IList<string> items && items.Contains(newValue))
        {
            transactionTypePicker.SelectedItem = newValue;
        }
    }
    private async void listView_ItemTapped(object sender, ItemTappedEventArgs e)
    {
        var item = (PaymentTransaction)e.Item;
        var action = await DisplayActionSheet("Action", "Cancel", null, "Edit", "Delete");
        switch (action)
        {
            case "Edit":
                _editPaymentTransferRecordId = item.PaymentId;
                paymentIdEntryField.Text = item.PaymentId.ToString();
                itemCodeEntryField.Text = item.ItemCode;    
                UpdateTransactionTypePicker(item.TransactionType);
                amountEntryField.Text = item.Amount.ToString();
                _editCreateUpdateDate = DateTime.Now;
                break;
            case "Delete":
                await _dbService.DeletePaymentTransaction(item);
                // TODO: needs refresh the view
                break;
        }
    }

    private async void detailSaveButton_Clicked(object sender, EventArgs e)
    {
        String payment_str = amountEntryField.Text;
        float payment_float = float.Parse(payment_str);
        String selectedPaymentType = "Cash";
        if (transactionTypePicker.SelectedIndex != -1)
        {
            selectedPaymentType = transactionTypePicker.SelectedItem.ToString();
        }

        // Add PaymentTransaction
        await _dbService.CreatePaymentItem(new PaymentTransaction
        {
            ItemCode = itemCodeEntryField.Text,
            TransactionType = selectedPaymentType,
            Amount = payment_float,
            Created = DateTime.Now
        });
        await DisplayAlert("Payment Saved:", payment_float.ToString(), "OK");
    }

    private async void savePaymentTransactionButton_Clicked(object sender, EventArgs e)
    {
        if (_editPaymentTransferRecordId == 0)
        {
            detailSaveButton_Clicked(sender, e);
        } else
        {
            String payment_str = amountEntryField.Text;
            float payment_float = float.Parse(payment_str);
            String selectedPaymentType = "Cash";
            if (transactionTypePicker.SelectedIndex != -1)
            {
                selectedPaymentType = transactionTypePicker.SelectedItem.ToString();
            }
            // Add PaymentTransaction
            await _dbService.UpdatePaymentItem(new PaymentTransaction
            {
                PaymentId = _editPaymentTransferRecordId,
                ItemCode = itemCodeEntryField.Text,
                TransactionType = transactionTypePicker.SelectedItem.ToString(),
                Amount = payment_float,
                Updated = DateTime.Now
            });
            _editPaymentItemId = 0;
        }
        Task.Run(async () => listView.ItemsSource = await _dbService.GetItemsNotDoneAsync("c0", "Card","2025-01-19"));
        Dispatcher.DispatchAsync(async () =>
        {
            await DisplayAlert("Saving the detail and updating totals with ", "", "OK");
        });
    }

    
    private void OnPickerSelectedIndexChanged(object sender, EventArgs e)
    {
        if (transactionTypePicker is not null)
        {
            if (transactionTypePicker.SelectedIndex != -1)
            {
                string selectedPaymentType = transactionTypePicker.SelectedItem.ToString();
            }
        }
    }
    
    private void OnEntryUnfocused(object sender, FocusEventArgs e)
    {
        if (double.TryParse(amountEntryField.Text, out double amount))
        {
            amountEntryField.Text = amount.ToString("F2");
        }
    } 
}