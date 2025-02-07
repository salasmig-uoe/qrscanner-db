using QRScanner.Database;
using QRScanner.Services;
using QRScanner.ViewModel;
using System.Net.Mail;
using System.Net;
using System.Net.Mime;
using System.Text;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using DocumentFormat.OpenXml.Drawing;
using Run = DocumentFormat.OpenXml.Wordprocessing.Run;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Primitives;


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


    private async Task<string> Get_day_transactionAsync(string formattedDate)
    {
        var items = await _dbService.GetPaymentTransferByDate(formattedDate);
        string result = "";
        foreach (var item in items)
            result += item.PaymentId + ";" + item.TransactionCode + ";" + item.ItemCode + ";" + item.Quantity + ";" + item.Amount + ";" + item.TransactionType + ";" + item.Created + ";" + item.Updated + "\n";

        return result;
    }

    private async void OnEmailButtonClicked(object sender, EventArgs e)
    {
        bool automatic = true; // TODO email(4)

        if (automatic == false)
        {
            Navigation.PushAsync(new QRScanner.Pages.EmailSaleTransactionsPage(_emailViewModel));
        }
        else
        {
            try
            {
                // Email details
                string fromEmail = "salasmig@gmail.com"; //TODO email(1) Your Gmail address
                string toEmail = "salasmig@yahoo.com.mx"; // Recipient's email
                string subject = "New Test Email from .NET MAUI";
                string body = "This is an automated email sent from a .NET MAUI app.";

                // Gmail SMTP settings
                string smtpHost = "smtp.gmail.com";
                int smtpPort = 587;
                string smtpUsername = "salasmig@gmail.com";// TODO email(2) UserEntry.Text; // Your Gmail address
                string smtpPassword = "nlwf hqwt lmcm vqkb";// AccessCodeEntry.Text; // Your App Password or Gmail password

                // Create the email message
                MailMessage mail = new MailMessage(fromEmail, toEmail, subject, body);


                string transactions = await Get_day_transactionAsync("2025-02-07");

                // String to be attached as a text file
                string attachmentContent = transactions;
                string attachmentFileName = "attachment.txt";


                // testing replacing image
                replaceImage();



                // Convert the string to a MemoryStream
                byte[] attachmentBytes = Encoding.UTF8.GetBytes(attachmentContent);
                MemoryStream attachmentStream = new MemoryStream(attachmentBytes);

                // Create the attachment
                Attachment attachment = new Attachment(attachmentStream, attachmentFileName, MediaTypeNames.Text.Plain);
                mail.Attachments.Add(attachment);

                // Set up the SMTP client
                SmtpClient smtpClient = new SmtpClient(smtpHost, smtpPort)
                {
                    EnableSsl = true,
                    Credentials = new NetworkCredential(smtpUsername, smtpPassword)
                };

                // Send the email
                smtpClient.Send(mail);

                // Update the status label
                await Application.Current.MainPage.DisplayAlert("Status: Email sent successfully!", "email sent", "OK");
            }
            catch (Exception ex)
            {
                // Handle errors
                string StatusLabel = $"Status: Error - {ex.Message}";
                await Application.Current.MainPage.DisplayAlert(StatusLabel, "email not sent", "OK");
            }
        }
    }
    /*
    private async void OnEmailButtonClicked(object sender, EventArgs e)
    {
        bool automatic = false;

        if (automatic == false)
        {
            Navigation.PushAsync(new QRScanner.Pages.EmailSaleTransactionsPage(_emailViewModel));
        }
        else
        {
            try
            {
                // Email details
                string fromEmail = "youraccount@gmail.com"; // Your Gmail address
                string toEmail = "youraccount@yahoo.com.mx"; // Recipient's email
                string subject = "Test Email from .NET MAUI";
                string body = "This is an automated email sent from a .NET MAUI app.";

                // Gmail SMTP settings
                string smtpHost = "smtp.gmail.com";
                int smtpPort = 587;
                string smtpUsername = ""; // UserEntry.Text; // Your Gmail address
                string smtpPassword = ""; // AccessCodeEntry.Text; // Your App Password or Gmail password

                // Create the email message
                MailMessage mail = new MailMessage(fromEmail, toEmail, subject, body);

                // Set up the SMTP client
                SmtpClient smtpClient = new SmtpClient(smtpHost, smtpPort)
                {
                    EnableSsl = true,
                    Credentials = new NetworkCredential(smtpUsername, smtpPassword)
                };

                // Send the email
                smtpClient.Send(mail);

                // Update the status label
                await Application.Current.MainPage.DisplayAlert("Status: Email sent successfully!", "email sent", "OK");
            }
            catch (Exception ex)
            {
                // Handle errors
                string StatusLabel= $"Status: Error - {ex.Message}";
                await Application.Current.MainPage.DisplayAlert(StatusLabel, "email not sent", "OK");
            }
        }
    }
    */
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

    private static string AddImageToDocument(MainDocumentPart mainPart, byte[] imageBytes)
    {
        // Add the image part to the document
        ImagePart imagePart = mainPart.AddImagePart(ImagePartType.Png);
        using (MemoryStream stream = new MemoryStream(imageBytes))
        {
            imagePart.FeedData(stream);
        }

        // Return the relationship ID of the image
        return mainPart.GetIdOfPart(imagePart);
    }

    private static void ProcessParagraph(DocumentFormat.OpenXml.Wordprocessing.Paragraph paragraph, string token)
    {
        if (paragraph.InnerText.Contains(token))
        {
            int a = 0;
        }
    }
    private static void ReplaceTokenWithImage(MainDocumentPart mainPart, string token, string imageId)
    {
        // Iterate through all paragraphs in the document
        foreach (var paragraph in mainPart.Document.Descendants<DocumentFormat.OpenXml.Wordprocessing.Paragraph>())
        {
            // Check if the paragraph contains the token
            if (paragraph.InnerText.Contains(token))
            {
                // Remove the token
                paragraph.RemoveAllChildren<Run>();

                // Create a new run with the image
                Run run = new Run();
                Drawing drawing = CreateImageDrawing(imageId);
                run.AppendChild(drawing);

                // Add the run to the paragraph
                paragraph.AppendChild(run);
            }
        }

        // Check main body
        foreach (var paragraph in mainPart.Document.Body.Descendants<DocumentFormat.OpenXml.Wordprocessing.Paragraph>())
        {
            ProcessParagraph(paragraph, token);
        }

        // Check tables
        foreach (var table in mainPart.Document.Descendants<DocumentFormat.OpenXml.Wordprocessing.Table>())
        {
            foreach (var row in table.Descendants<DocumentFormat.OpenXml.Wordprocessing.TableRow>())
            {
                foreach (var cell in row.Descendants<DocumentFormat.OpenXml.Wordprocessing.TableCell>())
                {
                    foreach (var paragraph in cell.Descendants<DocumentFormat.OpenXml.Wordprocessing.Paragraph>())
                    {
                        ProcessParagraph(paragraph, token);
                    }
                }
            }
        }

        // Check headers
        foreach (var headerPart in mainPart.HeaderParts)
        {
            foreach (var paragraph in headerPart.Header.Descendants<DocumentFormat.OpenXml.Wordprocessing.Paragraph>())
            {
                ProcessParagraph(paragraph, token);
            }
        }

        // Check footers
        foreach (var footerPart in mainPart.FooterParts)
        {
            foreach (var paragraph in footerPart.Footer.Descendants<DocumentFormat.OpenXml.Wordprocessing.Paragraph>())
            {
                ProcessParagraph(paragraph, token);
            }
        }

        mainPart.Document.Save();
        Console.WriteLine("Processing complete.");


    }

    private static Drawing CreateImageDrawing(string imageId)
    {
        // Define the image size (you can adjust these values)
        long width = 1000000; // Width in EMUs (English Metric Units)
        long height = 1000000; // Height in EMUs

        // Create the drawing element
        Drawing drawing = new Drawing(
            new DocumentFormat.OpenXml.Drawing.Wordprocessing.Inline(
                new DocumentFormat.OpenXml.Drawing.Wordprocessing.Extent() { Cx = width, Cy = height },
                new DocumentFormat.OpenXml.Drawing.Wordprocessing.EffectExtent() { LeftEdge = 0L, TopEdge = 0L, RightEdge = 0L, BottomEdge = 0L },
                new DocumentFormat.OpenXml.Drawing.Wordprocessing.DocProperties() { Id = 1U, Name = "Picture 1" },
                new DocumentFormat.OpenXml.Drawing.Wordprocessing.NonVisualGraphicFrameDrawingProperties(
                    new DocumentFormat.OpenXml.Drawing.GraphicFrameLocks() { NoChangeAspect = true }),
                new DocumentFormat.OpenXml.Drawing.Graphic(
                    new DocumentFormat.OpenXml.Drawing.GraphicData(
                        new DocumentFormat.OpenXml.Drawing.Pictures.Picture(
                            new DocumentFormat.OpenXml.Drawing.Pictures.NonVisualPictureProperties(
                                new DocumentFormat.OpenXml.Drawing.Pictures.NonVisualDrawingProperties() { Id = 0U, Name = "Picture 1" },
                                new DocumentFormat.OpenXml.Drawing.Pictures.NonVisualPictureDrawingProperties()),
                            new DocumentFormat.OpenXml.Drawing.Pictures.BlipFill(
                                new DocumentFormat.OpenXml.Drawing.Blip() { Embed = imageId },
                                new DocumentFormat.OpenXml.Drawing.Stretch(
                                    new DocumentFormat.OpenXml.Drawing.FillRectangle())),
                            new DocumentFormat.OpenXml.Drawing.Pictures.ShapeProperties(
                                new DocumentFormat.OpenXml.Drawing.Transform2D(
                                    new DocumentFormat.OpenXml.Drawing.Offset() { X = 0L, Y = 0L },
                                    new DocumentFormat.OpenXml.Drawing.Extents() { Cx = width, Cy = height }),
                                new DocumentFormat.OpenXml.Drawing.PresetGeometry(
                                    new DocumentFormat.OpenXml.Drawing.AdjustValueList())
                                { Preset = DocumentFormat.OpenXml.Drawing.ShapeTypeValues.Rectangle }))
                    )
                    { Uri = "http://schemas.openxmlformats.org/drawingml/2006/picture" }))
        );

        return drawing;
    }
    private async void replaceImage()
    {
        // Path to the Word document
        string documentPath = "C:\\Users\\msalasz\\Documents\\docwithimgage.docx";

        // Path to the image you want to insert
        string imagePath = "C:\\Users\\msalasz\\Downloads\\gatofrio.png";

        // Token to be replaced by the image
        string token = "IMAGE_TOKEN";


        // Open the Word document
        using (WordprocessingDocument document = WordprocessingDocument.Open(documentPath, true))
        {
            // Get the main document part
            MainDocumentPart mainPart = document.MainDocumentPart;

            // Load the image into a byte array
            byte[] imageBytes = File.ReadAllBytes(imagePath);

            // Add the image to the document and get its relationship ID
            string imageId = AddImageToDocument(mainPart, imageBytes);

            // Find and replace the token with the image
            ReplaceTokenWithImage(mainPart, token, imageId);

            // Save the document
            mainPart.Document.Save();

            Console.WriteLine("Image inserted successfully!");


        }
    }
}