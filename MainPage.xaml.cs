using System.Globalization;
using QRScanner.ViewModel;
using ZXing.QrCode.Internal;
using static System.Net.Mime.MediaTypeNames;
using static System.Runtime.CompilerServices.RuntimeHelpers;

namespace QRScanner
{
    public partial class MainPage : ContentPage
    {
        private readonly LocalDbService _dbService;
        private int _editArtItemId;
        private int _editPaymentItemId;
        private int _capturedItem;

        public MainPage(LocalDbService dbService, MainViewModel vm)
        {
            InitializeComponent();
            BindingContext = vm;

            _capturedItem = 0;
            
            codeReader.Options = new ZXing.Net.Maui.BarcodeReaderOptions
            {
                Formats = ZXing.Net.Maui.BarcodeFormat.QrCode,
                AutoRotate = true,
                Multiple = true
            };
            
            
            _dbService = dbService;
            Task.Run(async () => listView.ItemsSource = await _dbService.GetArtItems());

        }

        private void analyseContent(String strcode)
        {
            String[] fields = strcode.Split(':');
            MainThread.BeginInvokeOnMainThread(() =>
            {
                String full_text = "";
                for (int i = 0; i < fields.Length; i++)
                {
                    String field_value = fields[i].Trim();
                    switch (i)
                    {
                        case 0:
                            artist_codeEntryField.Text = field_value;
                            break;
                        case 1:
                            titleEntryField.Text = field_value;
                            amountEntryField.Text = field_value;
                            full_text = "Title:   " + field_value + "    ";
                            break;
                        case 2:
                            typeEntryField.Text = field_value;
                            full_text += "Media:    " + field_value + "     ";
                            break;
                        case 3:
                            full_text += "Dimensions:    " + field_value + " (cm)";
                            break;
                        case 4:
                            priceEntryField.Text = field_value;
                            break;
                        case 6:
                            item_codeEntryField.Text = field_value;                            
                            transaction_typeEntryField.Text = field_value;
                            break;
                    }
                }
                amountEntryField.Text = full_text;
            });
        }

        private void codeReader_BarCodesDetected(object sender, ZXing.Net.Maui.BarcodeDetectionEventArgs e)
        {
            var first = e.Results?.FirstOrDefault();
            if (first is null)
                return;

            // It is waiting to capture the following Item
            if (_capturedItem == 0)
            {

                //Split the code

                _capturedItem = 1;

                //analyseContent(sender, first.Value);
                String strcode = first.Value;

                analyseContent(strcode);

                Dispatcher.DispatchAsync(async () =>
                {
                    await DisplayAlert("Barcode Detected", first.Value, "OK");
                });

            }

        }

        private async void capturedButton_Clicked(object sender, EventArgs e)
        {
            _capturedItem = 0;
            
           item_codeEntryField.Text = string.Empty;
           priceEntryField.Text = string.Empty;

           Dispatcher.DispatchAsync(async () =>
            {
                await DisplayAlert("The capture is now enabled", _capturedItem.ToString(), "OK");
            });
        }

        private void OnPickerSelectedIndexChanged(object sender, EventArgs e)
        {
            if (picker is not null)
            {
                if (picker.SelectedIndex != -1)
                {
                    string selectedPaymentType = picker.Items[picker.SelectedIndex];
                    // Use the selectedPaymentType as needed
                }
            }
        }

        private void OnEntryUnfocused(object sender, FocusEventArgs e)
        {
            if (double.TryParse(detail_amount_EntryField.Text, out double amount))
            {
                detail_amount_EntryField.Text = amount.ToString("F2");
            }
        }
        private async void detailSaveButton_Clicked(object sender, EventArgs e)
        {
            String payment_str = detail_amount_EntryField.Text;
            float payment_float = float.Parse(payment_str);
            String selectedPaymentType = "Cash";
            if (picker.SelectedIndex != -1)
            {
                selectedPaymentType = picker.Items[picker.SelectedIndex];
                // Use the selectedPaymentType as needed
            }

            // Add PaymentTransaction
            await _dbService.CreatePaymentItem(new PaymentTransaction
            {
                ItemCode = item_codeEntryField.Text,
                TransactionType = selectedPaymentType,
                Amount = payment_float
            });
            await DisplayAlert("Payment Saved:", payment_float.ToString(), "OK");
        }

        private async void saveTransactionButton_Clicked(object sender, EventArgs e)
        {
            MainThread.BeginInvokeOnMainThread(() => {
                transaction_typeEntryField.Text = string.Empty;
                amountEntryField.Text = string.Empty;
                titleEntryField.Text = string.Empty;
                item_codeEntryField.Text = string.Empty;
                typeEntryField.Text = string.Empty;
                artist_codeEntryField.Text = string.Empty;
                priceEntryField.Text = string.Empty;
            });


            if (_editPaymentItemId == 0)
            {
                // Add PaymentTransaction
                await _dbService.CreatePaymentItem(new PaymentTransaction
                {
                    ItemCode = item_codeEntryField.Text,
                    TransactionType = "card",
                    Amount = 100
                });                
            }

            /*
            float _total_sofar = 100;

            float float_amount = 0;
            if (item_codeEntryField.Text is null)
            {
                await DisplayAlert("No Art Item to apply the payment", float_amount.ToString(), "OK");
                return;
            }
            String str_amount = amountEntryField.Text;
            float_amount = float.Parse(str_amount, CultureInfo.InvariantCulture.NumberFormat);

            if (_editPaymentItemId == 0)
            {
                // Add PaymentTransaction
                await _dbService.CreatePaymentItem(new PaymentTransaction
                {
                    ItemCode = item_codeEntryField.Text,
                    TransactionType = transaction_typeEntryField.Text[0],
                    Amount = float_amount
                }); 
            } else
            {
                // Add PaymentTransaction
                await _dbService.UpdatePaymentItem(new PaymentTransaction
                {
                    PaymentId = _editPaymentItemId,
                    ItemCode = item_codeEntryField.Text,
                    TransactionType = transaction_typeEntryField.Text[0],
                    Amount = float_amount
                });
                _editPaymentItemId = 0;
            }

            Dispatcher.DispatchAsync(async () =>
            {
                await DisplayAlert("Saving the detail and updating totals", _total_sofar.ToString(), "OK");
            });
            */

        }

        private async void saveButton_Clicked(object sender, EventArgs e)
        {
            if (_editArtItemId == 0)
            {
                // Add ArtItem
                String str_amount = priceEntryField.Text;
                float amount = float.Parse(str_amount.ToString(), CultureInfo.InvariantCulture.NumberFormat);
                await _dbService.Create(new ArtItem
                {
                    Title=titleEntryField.Text,
                    Type=typeEntryField.Text,
                    ArtistCode=artist_codeEntryField.Text,
                    Amount = amount,
                    Price = amount,
                    ItemCode = item_codeEntryField.Text,

                });
            }
            else
            {
                // Edit ArtItem
                String str_amount = priceEntryField.Text;
                float amount = float.Parse(str_amount.ToString(), CultureInfo.InvariantCulture.NumberFormat);

                await _dbService.Update(new ArtItem
                {
                    Id = _editArtItemId,
                    Title = titleEntryField.Text,
                    Type = typeEntryField.Text,
                    ArtistCode = artist_codeEntryField.Text,
                    Amount = amount,
                    Price = amount,
                    ItemCode =item_codeEntryField.Text,
                });
                _editArtItemId = 0;
            }
            listView.ItemsSource = await _dbService.GetArtItems();
        }

        private async void listView_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            var item =  (ArtItem)e.Item;
            var action = await DisplayActionSheet("Action", "Cancel", null, "Edit", "Delete");
            switch (action)
            {
                case "Edit":
                    _editArtItemId = item.Id;
                    titleEntryField.Text = item.Title;
                    typeEntryField.Text = item.Type;
                    artist_codeEntryField.Text = item.ArtistCode;
                    priceEntryField.Text = item.Price.ToString();
                    break;
                case "Delete":
                    await _dbService.Delete(item);
                    listView.ItemsSource = await _dbService.GetArtItems();
                    break;
            }
        }
        
    }
}
