
using Application.Abstractions;
using Serilog;
using iTextSharp.text.pdf;
using iTextSharp.text;
namespace Application.Services
{
    public class PdfGenerator : IPdfGenerator
    {
        public async Task<byte[]> GeneratePdfAsync(string content)
        {
            try
            {
                Log.Information("Generating PDF document");

                using (var memoryStream = new MemoryStream())
                {
                    // create document 
                    var document = new Document(PageSize.A4, 25, 25, 30, 30);
                    var writer = PdfWriter.GetInstance(document, memoryStream);

                    document.Open();

                    // Add content
                    var paragraph = new Paragraph(content)
                    {
                        Font = FontFactory.GetFont(FontFactory.HELVETICA, 12)
                    };

                    document.Add(paragraph);
                    document.Close();

                    Log.Debug("Successfully generated PDF");
                    return await Task.FromResult(memoryStream.ToArray());
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error occurred while generating PDF");
                throw;
            }
        }
    }
}
