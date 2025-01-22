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

        // Set values manually
        ItemCodeLabel.Text = "000000000";
        ArtistCodeLabel.Text = "Artist0000000";
        TitleLabel.Text = "Title";
        MaterialLabel.Text = "";
        DimensionsLabel.Text = "";
        PriceLabel.Text = "0";

        MessagingCenter.Subscribe<PopupPage, ArtistItemData>(this, "UpdateControls", (sender, data) =>
        {
            // Updating the Headers
            ItemCodeLabel.Text = data.ItemCodeLabel;
            ArtistCodeLabel.Text = data.ArtistCodeLabel;
            TitleLabel.Text = data.TitleLabel;
            MaterialLabel.Text = data.MaterialLabel;
            DimensionsLabel.Text = data.DimensionsLabel;
            PriceLabel.Text = data.PriceLabel;
            // Updating the payment row
            itemCodeEntryField.Text = data.ItemCodeLabel;
            quantityEntryField.Text = "1";
            amountEntryField.Text = data.PriceLabel;
            // Execute the list update
            // TODO check why is not updating the listView 
            Task.Run(async () => listView.ItemsSource = await _dbService.GetItemsNotDoneAsync(data.ItemCodeLabel, "2025-01-22"));
        });

        Task.Run(async () => listView.ItemsSource = await _dbService.GetItemsNotDoneAsync("KD2022001", "2025-01-22"));
    }


    public void generateNextTransactionGroupButton_Clicked(object sender, EventArgs e)
    {
        String str_today = DateTime.Now.ToString("ddMMyy");
        String new_key = "";
        String old_key = transaction_group_EntryLabel.Text; 
        if (old_key is null || old_key.Trim() == "")
            new_key = str_today + "-00001";
        else
            new_key = str_today + "-00002";
        transaction_group_EntryLabel.Text = new_key;
        transactionGroupIdEntryField.Text = new_key;
    }

    /*
     
    // TODO when a button in main screen is pressed, then unsubscribe
    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        MessagingCenter.Unsubscribe<PopupPage, ArtistItemData>(this, "UpdateControls");
    }
    */

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
        }
        else
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
        Task.Run(async () => listView.ItemsSource = await _dbService.GetItemsNotDoneAsync("c0","2025-01-19"));
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