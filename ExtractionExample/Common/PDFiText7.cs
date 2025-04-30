using Cysharp.Text;
using DelightfulCode;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Listener;
using System.Text;

namespace ExtractionExample
{
    /// <summary>
    /// Class for working with PDF (a layer over the PDFTech library).
    /// TODO: See if iText NUGET has improved abilities .... https://kb.itextpdf.com/home/it7kb/examples
    /// </summary>
    public static class PDFiText7
    {
        /// <summary>
        /// Creates a new text file from a given file.
        /// If successful, a reference to the file is returned.
        /// If null, you can check the file system for the error log.
        /// </summary>
        /// <param name="reportProgress">Optionally Write progress report to the console, if present (allows visual tracking of large batches), involves threading</param>
        public static FileInfo? ExtractText(string fullFileNamePathToPDF, bool reportProgress)
        {
            FileInfo fi = new FileInfo(fullFileNamePathToPDF);
            String folderName = fi.DirectoryName;
            Utf16ValueStringBuilder processed = ZString.CreateStringBuilder(); // This string builder is much lighter on memory usage

            string errorText = string.Empty; // reuse
            string text_this_page = string.Empty; // reuse
            string[] lines_this_page; // reuse

            PdfReader reader = new PdfReader(fullFileNamePathToPDF);
            PdfDocument pdf = new PdfDocument(reader);

            int total_pdf_pages = pdf.GetNumberOfPages();
            string pdfVersion = $"1.{pdf.GetPdfVersion()}";

            processed.AppendLine($"##### EXTRACTION: {fi.Name} DETECTED: version {pdfVersion} of the PDF specification. Extraction using iText7 NuGet package. #####");

            for (int current_page = 1; current_page <= total_pdf_pages; current_page++)
            {
                Common.ConsoleProgressDisplay(fi.Name, current_page, total_pdf_pages, "pages (itext7)");

                PdfPage content = pdf.GetPage(current_page);

                ITextExtractionStrategy strategy2 = new SimpleTextExtractionStrategy();

                text_this_page = PdfTextExtractor.GetTextFromPage(content, strategy2);

                lines_this_page = text_this_page.Split('\n');

                foreach (string line2 in lines_this_page)
                {
                    processed.Append(line2 + Environment.NewLine);
                }

                processed.Append($"{Environment.NewLine}{Environment.NewLine}### Page ###{current_page}### Finished #####{Environment.NewLine}{Environment.NewLine}");

                text_this_page = string.Empty;
            }

            pdf.Close();
            reader.Close();

            string directory = Path.GetDirectoryName(fullFileNamePathToPDF);
            string filenameWithoutExtension = Path.GetFileNameWithoutExtension(fullFileNamePathToPDF);
            string textfilenameFinal = fullFileNamePathToPDF.Replace(".pdf", ".(itext7).txt"); // assuming a pdf, that is.
            string textfilenameError = Path.Combine(directory, filenameWithoutExtension + ".error.txt");

            // Check  we don't have a collision on the text file. if so, then that's bad, and we will avoid.

            if (File.Exists(textfilenameFinal))
            {
                textfilenameFinal = textfilenameFinal.Replace(".txt", string.Format(".txt.(duplicate).{0:yyyyMMdd.HHmmss}.(itext7).txt", DateTime.Now));
                errorText += string.Format("Text file already processed, so duplicate text file generated: {0}{1}.", Environment.NewLine, textfilenameFinal);
            }

            // Finally, do a final flush to the file

            System.IO.File.WriteAllText(textfilenameFinal, processed.ToString(), System.Text.Encoding.UTF8); // this is supporting cryllic characters

            // THE SLV files need a slightly different processing format,
            // Due to a quirk in their presentation layout.
            // This should give a bit more of a reliable result.

            // Any error information encountered, should be fully written to a text file to deconstruct later.

            if (errorText.HasSomeValue())
            {
                StreamWriter sr2 = new StreamWriter(textfilenameError, false, Encoding.UTF8);
                sr2.WriteLine(errorText);
                sr2.Close();
                sr2.Dispose();
            }

            return new FileInfo(textfilenameFinal);
        }
    }
}
