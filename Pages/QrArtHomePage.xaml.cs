using QRScanner.Database;
using QRScanner.Services;
using QRScanner.ViewModel;
using System.Text.RegularExpressions;
namespace QRScanner.Pages;

public partial class QrArtHomePage : ContentPage
{
    LocalDbService _dbService;
    MainViewModel _vm;
    EditSaleTransactionsPage _editTransactionsPage;
    AlertService _alertService;
    public QrArtHomePage(LocalDbService dbService, MainViewModel vm,
        EmailViewModel emailViewModel, EditSaleTransactionsPage editTransactionsPage,
        AlertService alertService)
    {
        InitializeComponent();
        _dbService = dbService;
        _vm = vm;
        BindingContext = vm;
        _editTransactionsPage = editTransactionsPage;
        _alertService = alertService;
    }

    private async void OnSellingButtonClicked(object sender, EventArgs e)
    {
        Navigation.PushAsync(new QRScanner.Pages.CreateSaleTransactionsPage(_dbService, _vm));
    }

    private async void OnEditTransferButtonClicked(object sender, EventArgs e)
    {
        Navigation.PushAsync(new QRScanner.Pages.EditSaleTransactionsPage(_dbService, _vm));
    }

    private async void OnViewButtonClicked(object sender, EventArgs e)
    {
        Navigation.PushAsync(new QRScanner.Pages.ViewSaleTransactionsPage(_dbService, _vm));
    }

    private async Task ProcessTextFile(FileResult fileResult)
    {
        using var stream = File.OpenRead(fileResult.FullPath);
        using var reader = new StreamReader(stream);
        string fileContent = await reader.ReadToEndAsync();

        using var stringReader = new StringReader(fileContent);
        string line;
        int line_num = 0;
        string ArtistName = "";
        string ArtistCode = "";
        string ArtistEmail = "";

        while ((line = stringReader.ReadLine()) != null)
        {
            var tokens = line.Split(",");

            switch (line_num)
            {
                case 0:
                    // Artist name
                    ArtistName = tokens[1].Trim();
                    break;
                case 1:
                    // Artist code
                    ArtistCode = tokens[1].Trim();
                    break;
                case 2:
                    // Artist email
                    ArtistEmail = tokens[0].Trim();
                    break;
                case 3:
                    //Title headers
                    break;
                default:
                    // Artist items
                    ArtItem item = new ArtItem();
                    ArtistDetail artist = new ArtistDetail();

                    int field_num = 0;
                    foreach (var token in tokens)
                    {
                        switch (field_num)
                        {
                            case 0: item.Title = token.Trim(); break;
                            case 1: item.WorkType = token.Trim(); break;
                            case 2: item.Size = token.Trim(); break;
                            case 3: if (float.TryParse(token.Replace("\"", ""), out float price)) item.Price = price; break;
                            case 4: if (float.TryParse(token, out float amount)) item.Amount = amount; break;
                            case 5: item.ItemCode = token.Trim(); break;
                        }
                        field_num++;
                    }
                    item.ArtistName = ArtistName;
                    item.ArtistCode = ArtistCode;
                    item.Created = DateTime.Now;
                    item.Updated = DateTime.Now;
                    item.PriceBalance = item.Price;
                    item.AmountBalance = item.Amount;
                    item.LabelType = "w";
                    
                    string item_code = item.ItemCode;
                    var existing_item = await _dbService.GetByItemCode(item_code);
                    if (existing_item == null)
                    {
                        await _dbService.Create(item);
                    }

                    artist.ArtistName = ArtistName; 
                    artist.ArtistCode = ArtistCode;
                    artist.ArtistEmail = ArtistEmail;
                    var existing_artist = await _dbService.GetArtistAsync(artist.ArtistCode);
                    if (existing_artist == null)
                    {
                        await _dbService.CreateArtist(artist);
                    }
                    break;
            }
            line_num++;
        }
    }

    private async Task ProcessAllTextFilesInDirectory(string directoryPath)
    {
        // Get all txt files in the directory
        var txtFiles = Directory.GetFiles(directoryPath, "*.csv");

        foreach (var filePath in txtFiles)
        {
            // Open each file as a FileResult
            var fileResult = new FileResult(filePath);
            await ProcessTextFile(fileResult);
        }

        await Application.Current.MainPage.DisplayAlert("Files Loaded", $"All ({txtFiles.Length}) files in the directory have been loaded", "OK");
    }

    private async void UploadBackup() {
        try
        {
            var fileResult = await FilePicker.Default.PickAsync(new PickOptions
            {
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
                                case 14: item.LabelType = token; break;
                            }
                            field_num++;
                        }
                        item.PriceBalance = item.Amount*item.Price;
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
                                // the item exists and option is (0) don't do anything
                                case "0": break;
                                 // the item exists and the option is (1) then update all the info with loaded and calculated values
                                case "1":
                                    item.Created = existing_item.Created;
                                    await _dbService.Update(item);
                                    break;
                                // the item exists and the option is (2) only update the balances
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

    private async void OnToolsTransferButtonClicked(object sender, EventArgs e)
    {
        // TODO: Implement the backup and restore functionality
        // 1. Upload all the items in a desktop computer,
        // 2. Backup the DB import to csv
        // 3. Add the restore option in the header ¬
        // 4. Transfer to tablen with upload_bacukp = true
        bool upload_backup = false;
        if (upload_backup)
        {
            UploadBackup();
        }
        else
        {
            // TODO: Select directory path
            string directoryPath = "c://Users//msalasz//Downloads//input_files";
            if (!string.IsNullOrEmpty(directoryPath))
            {
                await ProcessAllTextFilesInDirectory(directoryPath);
            }
            
        }
    }

    private async void OnEmailButtonClicked(object sender, EventArgs e)
    {
        // Create the popup page
        var popup = new EmailSaleTransactionsPage(_alertService, _dbService);
        // Show the popup
        await Navigation.PushModalAsync(popup);
    }
    
    private async void OnDisplayGeneratorClicked(object sender, EventArgs e)
    {
        // Create the popup page
        var popup = new QrGeneratorPage(_dbService);
        // Show the popup
        await Navigation.PushModalAsync(popup);
    }
}