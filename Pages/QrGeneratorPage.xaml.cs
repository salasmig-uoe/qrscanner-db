using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using Run = DocumentFormat.OpenXml.Wordprocessing.Run;

using QRScanner.Services;
using QRCoder;
using Microsoft.Maui.Storage;

namespace QRScanner.Pages;

public partial class QrGeneratorPage : ContentPage
{
    private LocalDbService _dbService;
    public QrGeneratorPage(LocalDbService dbService)
    {
        InitializeComponent();
        _dbService = dbService;
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

    private static void ReplaceItemDetails(MainDocumentPart mainPart, Dictionary<string, string> tokenReplacements)
    {
        // Iterate through all paragraphs in the document
        foreach (var paragraph in mainPart.Document.Descendants<DocumentFormat.OpenXml.Wordprocessing.Paragraph>())
        {
            foreach (var run in paragraph.Elements<Run>())
            {
                foreach (var token in tokenReplacements.Keys)
                {
                    var textElement = run.GetFirstChild<Text>();
                    if (textElement != null && textElement.Text.Contains(token))
                    {
                        // Replace token with the corresponding text
                        textElement.Text = textElement.Text.Replace(token, tokenReplacements[token]);
                    }
                }
            }
        }
    }


    private static void ReplaceTokenWithImage(MainDocumentPart mainPart, string token, string imageId)
    {
        // Iterate through all paragraphs in the document
        foreach (var paragraph in mainPart.Document.Descendants<DocumentFormat.OpenXml.Wordprocessing.Paragraph>())
        {
            // Check if the paragraph contains the small_token
            if (paragraph.InnerText.Contains(token))
            {
                // Remove the small_token
                paragraph.RemoveAllChildren<Run>();

                // Create a new run with the image
                Run run = new Run();
                Drawing drawing = CreateImageDrawing(imageId);
                run.AppendChild(drawing);

                // Add the run to the paragraph
                paragraph.AppendChild(run);
            }
        }
        mainPart.Document.Save();
        Console.WriteLine("Processing complete.");
    }

    private static Drawing CreateImageDrawing(string imageId)
    {
        // Define the image size (you can adjust these values)
        long width = 914400; // One inch in EMUs (English Metric Units)
        long height = 914400; // One inch in EMUs

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

    private async void InsertItemsAndQRIntoDocument(string template_doc_path,
        string target_doc_path,
        string small_image_path,
        string large_image_path,
        Dictionary<string, string> tokenReplacements)
    {
        // Path to the image you want to insert
        string imagePath = small_image_path;

        // Token to be replaced by the image
        string small_token = "_SMALL_IMAGE";
        string large_token = "_LARGE_IMAGE";

        // Copy the original document to the new path
        File.Copy(template_doc_path, target_doc_path, true);

        // Path to the Word document
        string documentPath = target_doc_path;

        // Open the Word document
        using (WordprocessingDocument document = WordprocessingDocument.Open(documentPath, true))
        {
            // Get the main document part
            MainDocumentPart mainPart = document.MainDocumentPart;

            // Load the image into a byte array
            byte[] imageBytes = File.ReadAllBytes(imagePath);

            // Add the image to the document and get its relationship ID
            string imageId = AddImageToDocument(mainPart, imageBytes);

            // Find and replace the small_token with the image
            ReplaceTokenWithImage(mainPart, small_token, imageId);

            // Find and replace the large_token with the image
            ReplaceTokenWithImage(mainPart, large_token, imageId);

            ReplaceItemDetails(mainPart, tokenReplacements);

            // Save the document
            mainPart.Document.Save();
        }
    }

    public PngByteQRCode GenQrCode(string message)
    {
        QRCodeGenerator qrGenerator = new QRCodeGenerator();
        QRCodeData qrCodeData = qrGenerator.CreateQrCode(message, QRCodeGenerator.ECCLevel.L);

        PngByteQRCode qRCode = new PngByteQRCode(qrCodeData);
        return qRCode;
    }

    public void SaveQrCodeAsPng(string message, string filePath, PngByteQRCode qRCode, int size)
    {
        byte[] qrCodeBytes = qRCode.GetGraphic(size);

        // Save to file
        File.WriteAllBytes(filePath, qrCodeBytes);
    }


    private async Task prepare_dirsAsync(string word_template_path)
    {
        AlertService alertService = new AlertService();
        var items = await _dbService.GetArtItems();
        int num_items = items.Count;
        try
        {
            foreach (var item in items)
            {
                string curr_directory = FileSystem.AppDataDirectory;
                string artist_directory = Path.Combine(curr_directory, item.ArtistCode);
                string item_file = Path.Combine(artist_directory, $"{item.ItemCode}.docx");
                string image_directory = Path.Combine(artist_directory, "images");

                if (!Directory.Exists(artist_directory))
                    Directory.CreateDirectory(artist_directory);
                if (!Directory.Exists(image_directory))
                    Directory.CreateDirectory(image_directory);


                //string word_template = "";
                string qr_large = $"{item.ItemCode}_large.png";
                string qr_small = $"{item.ItemCode}_small.png";
                string word_name = $"{item.ItemCode}.docx";

                string data_for_code = $"{item.ArtistCode}:{item.ItemCode}:{item.Price}:{item.WorkType}:{item.Size}";
                data_for_code = $"{item.ItemCode}";
                // Filling the item information
                Dictionary<string, string> tokenReplacements = new Dictionary<string, string>();
                tokenReplacements.Add("_ARTIST", item.ArtistName);
                tokenReplacements.Add("_TITLE", item.Title);
                tokenReplacements.Add("_MEDIA", item.WorkType);
                tokenReplacements.Add("_SIZE", item.Size);
                tokenReplacements.Add("_PRICE", $"{item.Price:C2}");

                int small_size = 2;
                int large_size = 10;
                PngByteQRCode qRCode = GenQrCode(data_for_code);
                SaveQrCodeAsPng(data_for_code, Path.Combine(image_directory, qr_small), qRCode, small_size);
                SaveQrCodeAsPng(data_for_code, Path.Combine(image_directory, qr_large), qRCode, large_size);

                string template_doc_path = word_template_path;
                string target_doc_path = item_file;

                InsertItemsAndQRIntoDocument(template_doc_path, target_doc_path,
                    Path.Combine(image_directory, qr_small), Path.Combine(image_directory, qr_large),
                    tokenReplacements);

            }
            alertService.ShowMessageAsync("Success", $"QR Codes generated successfully for {num_items} items", "OK");
        }
        catch (Exception e)
        {
            alertService.ShowMessageAsync("Error", $"There was an error when processing items: {e.Message}", "OK");
            Console.WriteLine(e.Message);
        }
    }
    private async void OnSubmitClicked(object sender, EventArgs e)
    {
        var fileResult = await FilePicker.Default.PickAsync(new PickOptions
        {
            PickerTitle = "Select a text file"
        });
        if (fileResult != null)
        {
            string word_template_doc = fileResult.FullPath;
            await prepare_dirsAsync(word_template_doc);
        }
    }
    private void OnCancelClicked(object sender, EventArgs e)
    {
        // Close the popup on the UI thread
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            await App.Current.MainPage.Navigation.PopModalAsync();
        });
    }
}
