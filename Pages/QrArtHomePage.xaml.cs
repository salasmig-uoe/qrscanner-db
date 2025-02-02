using QRScanner.Database;
using QRScanner.Services;
using QRScanner.ViewModel;

namespace QRScanner.Pages;

public partial class QrArtHomePage : ContentPage
{
    LocalDbService _dbService;
    MainViewModel _vm;
    EmailViewModel _emailViewModel;
    EditSaleTransactionsPage _editTransactionsPage;
    public QrArtHomePage(LocalDbService dbService, MainViewModel vm, EmailViewModel emailViewModel, EditSaleTransactionsPage editTransactionsPage)
    {
        InitializeComponent();
        _dbService = dbService;
        _vm = vm;
        BindingContext = vm;
        _emailViewModel = emailViewModel;
        _editTransactionsPage = editTransactionsPage;
    }

    private async void OnSellingButtonClicked(object sender, EventArgs e)
    {
        Navigation.PushAsync(new QRScanner.Pages.CreateSaleTransactionsPage(_dbService, _vm));
    }

    private async void OnEmailButtonClicked(object sender, EventArgs e)
    {
        Navigation.PushAsync(new QRScanner.Pages.EmailSaleTransactionsPage(_emailViewModel));
    }

    private async void OnEditTransferButtonClicked(object sender, EventArgs e)
    {
        Navigation.PushAsync(new QRScanner.Pages.EditSaleTransactionsPage(_dbService, _vm));
    }

    private async void OnViewButtonClickedx(object sender, EventArgs e)
    {

        // Define the path to your Word document
        string inputFilePath = "c://Users//msalasz//template.docx";
        string outputFilePath = "c://Users//msalasz//output.docx";
        string newImagePath = "c://Users//msalasz//Downloads//sample_scan.jpg";

        // Create a dictionary with placeholders and their replacements
        Dictionary<string, string> replacements = new Dictionary<string, string>
            {
                { "Item_Artist", "The Artist" },
                { "Item_Title", "This is the painting name" },
                { "Item_Material", "The Material" },
                { "Item_Media", "The Media" },
                { "Item_Price", "£1,700.00" },
                { "Item_Code", "KD00001"   },
            };

        // Call the method to replace placeholders in the document
        WordDocumentHelper.ReplaceTablePlaceholders(inputFilePath, outputFilePath, newImagePath, replacements);
        //WordDocumentHelper wdh = new WordDocumentHelper();
    }

    private async void OnViewButtonClicked(object sender, EventArgs e)
    {
        Navigation.PushAsync(new QRScanner.Pages.ViewSaleTransactionsPage(_dbService, _vm));
    }
    private async void OnToolsTransferButtonClicked(object sender, EventArgs e)
    {
        try
        {
            var fileResult = await FilePicker.Default.PickAsync(new PickOptions
            {
                //FileTypes = FilePickerFileType.PlainText, // Restrict to .txt files
                PickerTitle = "Select a text file"
            });
            if (fileResult != null)
            {
                using var stream = await fileResult.OpenReadAsync();
                using var reader = new StreamReader(stream);
                string fileContent = await reader.ReadToEndAsync();

                using var stringReader = new StringReader(fileContent);
                string line;
                int line_num = 0;
                string update_option = "0";
                // (0) Just create, don't update if existing
                // (1) Update all data if exists
                // (2) Keep balances if exists

                while ((line = stringReader.ReadLine()) != null)
                {
                    if (line_num == 0)
                    {
                        var tokens = line.Split("¬");
                        if (tokens.Length > 1)
                            update_option = tokens[1];
                        else
                        {
                            await Application.Current.MainPage.DisplayAlert("Unexpected File Format", "file format is not recognised", "OK");
                            return;
                        }
                    }
                    else
                    {
                        ArtItem item = new ArtItem();
                        var tokens = line.Split(",");
                        int field_num = 0;

                        foreach (var token in tokens)
                        {
                            switch (field_num)
                            {
                                case 1: item.Title = token; break;
                                case 2: item.WorkType = token; break;
                                case 3: item.Size = token; break;
                                case 4: item.ArtistCode = token; break;
                                case 5: item.ArtistName = token; break;
                                case 6: if (float.TryParse(token, out float price)) item.Price = price; break;
                                case 7: if (float.TryParse(token, out float amount)) item.Amount = amount; break;
                                case 11: item.ItemCode = token; break;
                                case 12: item.Created = DateTime.Now; break;
                                case 13: item.Updated = DateTime.Now; break;
                            }
                            field_num++;
                        }
                        item.PriceBalance = item.Price;
                        item.AmountBalance = item.Amount;

                        string item_code = item.ItemCode;
                        var existing_item = await _dbService.GetByItemCode(item_code);
                        if (existing_item == null)
                        {
                            await _dbService.Create(item);
                        }
                        else
                        {
                            item.Id = existing_item.Id;
                            switch (update_option)
                            {
                                case "0": break;
                                case "1":
                                    item.Created = existing_item.Created;
                                    await _dbService.Update(item);
                                    break;
                                case "2":
                                    item.PriceBalance = existing_item.PriceBalance;
                                    item.AmountBalance = existing_item.AmountBalance;
                                    item.Created = existing_item.Created;
                                    await _dbService.Update(item);
                                    break;
                            }
                        }
                    }
                    line_num++;
                    Console.WriteLine(line); // Print or process each line
                }
                // Display or use the file content
                await Application.Current.MainPage.DisplayAlert("File Loaded", fileContent, "OK");
            }
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert("Error", ex.Message, "OK");
        }
    }
}