
using QRScanner.Pages;
using QRScanner.Popups;
using QRScanner.Services;
using QRScanner.ViewModel;

namespace QRScanner.Pages;

public partial class SellingRecordPage : ContentPage
{
    LocalDbService _dbService;
    MainViewModel _vm;
    PopupResult _result;
    EmailViewModel _emailViewModel;
    EditTransactionsPage _editTransactionsPage;
    public SellingRecordPage(LocalDbService dbService, MainViewModel vm, PopupResult result, EmailViewModel emailViewModel, EditTransactionsPage editTransactionsPage)	
    {
		InitializeComponent();
        _dbService = dbService;
        _vm = vm;
        BindingContext = vm;
        _result = result;
        _emailViewModel = emailViewModel;
        _editTransactionsPage = editTransactionsPage;
    }

    private async void OnSellingButtonClicked(object sender, EventArgs e)
    {
        Navigation.PushAsync(new QRScanner.Pages.DetailViewPage(_dbService, _vm,_result));
    }

    private async void OnEmailButtonClicked(object sender, EventArgs e)
    {
        Navigation.PushAsync(new QRScanner.Pages.EmailFormPage(_emailViewModel)); 
    }

    private async void OnEditTransferButtonClicked(object sender, EventArgs e)
    {
        Navigation.PushAsync(new QRScanner.Pages.EditTransactionsPage(_dbService, _vm));
    }

    private async void OnViewButtonClicked(object sender, EventArgs e)
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
        WordDocumentHelper.ReplaceTablePlaceholders(inputFilePath,outputFilePath,newImagePath,replacements);
        //WordDocumentHelper wdh = new WordDocumentHelper();

    }
}