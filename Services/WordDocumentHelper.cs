using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;

namespace QRScanner.Services
{
    public static class WordDocumentHelper
    {
        public static void ReplaceTablePlaceholders(string inputFilePath, string outputFilePath,
            string newImagePath, Dictionary<string, string> replacements)
        {

            // Copy the original file to the new file path
            File.Copy(inputFilePath, outputFilePath, true);

            using (WordprocessingDocument wordDoc = WordprocessingDocument.Open(outputFilePath, true))
            {
                var docBody = wordDoc.MainDocumentPart.Document.Body;
                var mainPart = wordDoc.MainDocumentPart;

                // Iterate through every table in the document
                foreach (Table table in docBody.Elements<Table>())
                {
                    foreach (TableRow row in table.Elements<TableRow>())
                    {
                        foreach (TableCell cell in row.Elements<TableCell>())
                        {
                            // Check each paragraph in the cell
                            foreach (Paragraph para in cell.Elements<Paragraph>())
                            {
                                foreach (var run in para.Elements<Run>())
                                {
                                    foreach (var text in run.Elements<Text>())
                                    {
                                        // Replace the text if it matches a placeholder
                                        if (replacements.ContainsKey(text.Text))
                                        {
                                            text.Text = replacements[text.Text];
                                        }
                                    }
                                }
                            }
                        }
                    }
                }


                // Find the first image in the document
                ImagePart oldImagePart = mainPart.ImageParts.FirstOrDefault();

                if (oldImagePart != null)
                {
                    // Remove the old image part
                    mainPart.DeletePart(oldImagePart);

                    // Add a new image part
                    ImagePart newImagePart = mainPart.AddImagePart(ImagePartType.Jpeg);

                    // Copy the new image into the new image part
                    using (FileStream stream = new FileStream(newImagePath, FileMode.Open))
                    {
                        newImagePart.FeedData(stream);
                    }

                    // Get the ID of the new image part
                    string newImagePartId = mainPart.GetIdOfPart(newImagePart);

                    // Find all Blip elements that reference the old image and update them to reference the new image
                    foreach (var blip in mainPart.Document.Descendants<DocumentFormat.OpenXml.Drawing.Blip>())
                    {
                        if (blip.Embed == mainPart.GetIdOfPart(oldImagePart))
                        {
                            blip.Embed = newImagePartId;
                        }
                    }
                }


                // Save the changes to the document
                wordDoc.MainDocumentPart.Document.Save();
            }
        }
    }
}
