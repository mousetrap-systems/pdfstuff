using Cysharp.Text;
using DelightfulCode;
using System.Text;
using System.Xml.Linq;
using UglyToad.PdfPig.Content;
using UglyToad.PdfPig.DocumentLayoutAnalysis;
using UglyToad.PdfPig.DocumentLayoutAnalysis.PageSegmenter;
using UglyToad.PdfPig.DocumentLayoutAnalysis.TextExtractor;

namespace ExtractionExample
{
    /// <summary>
    /// Class for working with PDF using 'pdfpig' library, for comparison
    /// </summary>
    public static class PDFPIG
    {
        /// <summary>
        /// Creates a new text file from a given file.
        /// If successful, a reference to the file is returned.
        /// If null, you can check the file system for the error log.
        /// </summary>
        /// <param name="reportProgress">Optionally Write progress report to the console, if present (allows visual tracking of large batches), involves threading</param>
        /// <param name="useVerticalBlocks">Special 'vertical' mode to capture column based data (your mileage may vary, check document layout)</param>
        public static FileInfo? ExtractText(string fullFileNamePathToPDF, bool reportProgress, bool useVerticalBlocksLayout)
        {
            FileInfo fi = new FileInfo(fullFileNamePathToPDF);

            String folderName = fi.DirectoryName;
            Utf16ValueStringBuilder processed = ZString.CreateStringBuilder(); // This is much lighter on memory usage

            string errorText = string.Empty; // reuse
            string text_this_page = string.Empty; // reuse
            string[] lines_this_page; // reuse

            int total_pdf_pages = 0;

            // First, validation - we must be very strict. We won't bother extracting known duplicates

            using (UglyToad.PdfPig.PdfDocument pdf = UglyToad.PdfPig.PdfDocument.Open(fi.FullName))
            {
                processed.AppendLine($"##### EXTRACTION: {fi.Name} DETECTED: version {pdf.Version} of the PDF specification. Extraction using PdfPig Nuget Package. #####");

                total_pdf_pages = pdf.NumberOfPages;

                if (pdf.TryGetXmpMetadata(out XmpMetadata metadata))
                {
                    XDocument xmp = metadata.GetXDocument();
                }
                else
                {
                    errorText += $"ERROR: Could not extract XmpMetadata from '{fi.Name}' (using PdfPig)";
                }

                for (int current_page = 1; current_page <= total_pdf_pages; current_page++)
                {
                    Common.ConsoleProgressDisplay(fi.Name, current_page, total_pdf_pages, "pages (PdfPig)");
                    UglyToad.PdfPig.Content.Page page = pdf.GetPage(current_page);

                    // IEnumerable<Word> words = page.GetWords(); // Or based on grouping letters into words.

                    // IEnumerable<IPdfImage> images = page.GetImages(); // you can also extract images too ...

                    // string rawText = page.Text; // Or the raw text of the page's content stream.

                    // Either extract based on order in the underlying document with newlines and spaces.
                    // ContentOrderTextExtractor.Options options = new ContentOrderTextExtractor.Options();
                    // options.ReplaceWhitespaceWithSpace;

                    if (useVerticalBlocksLayout == false)
                    {
                        // Normal PDF file content ... sequential

                        text_this_page = ContentOrderTextExtractor.GetText(page, true);

                        lines_this_page = text_this_page.Split('\n');

                        foreach (string line2 in lines_this_page)
                        {
                            // "Running Total 28,836.425 28,787.441\r"
                            // "Brand Bar No. Shape Assay Gross Ounces Fine Ounces Vault\r"
                            // "Printed On: 08 Dec 2022 Page 2 of 7"

                            processed.Append(line2);
                        }
                    }
                    else
                    {
                        // This is a special block format for columns which (depending on the PDF writer) unfortunately groups it together in vertical blocks.
                        // we have to grab EACH block and rework the content based on the line position.

                        IEnumerable<Word> words = page.GetWords();
                        VerticalDataBucket bucket = new VerticalDataBucket();

                        // Use default parameters
                        // - mode of letters' height and width used as gap size
                        // - no minimum block width

                        IReadOnlyList<TextBlock> blocks = RecursiveXYCut.Instance.GetBlocks(words);

                        foreach (TextBlock block in blocks)
                        {
                            // Do something with these blocks ... pass them to external processing,
                            // requires to split the main columns and turn them all into

                            IReadOnlyList<TextLine> lines = block.TextLines;
                            string blah = block.Text;

                            // Note: the text boxes are from left to right ...

                            if (block.IsDataCandidate())
                            {
                                bucket.Add(block.TextLines);
                            }
                            else
                            {
                                // put it into the file, BUT prefix it with the equivalent of a REM command...
                                processed.Append($"##### {block.Text}");
                            }

                            // extract a line ...
                            // processed.Append(line2);
                        }

                        processed.Append(Environment.NewLine);
                        processed.Append(Environment.NewLine);

                        // when we have finished the page,
                        // we can output from the data bucket ...
                        processed.Append(bucket.Dump());

                    }

                    processed.AppendLine($"{Environment.NewLine}{Environment.NewLine}##### ### Page ###{current_page}### Finished #####{Environment.NewLine}{Environment.NewLine}");

                    text_this_page = string.Empty; // flatten it ...
                }
            }

            // Write the resulted string into an output file in the working directory using UTF-8 encoding

            string directory = Path.GetDirectoryName(fullFileNamePathToPDF);
            string filenameWithoutExtension = Path.GetFileNameWithoutExtension(fullFileNamePathToPDF);
            string textfilenameFinal = fullFileNamePathToPDF.Replace(".pdf", ".(PdfPig).txt"); // assuming a pdf, that is.
            string textfilenameError = Path.Combine(directory, filenameWithoutExtension + ".error.txt");

            // Check that we don't have a collision on the text file. if so, then that's bad, and we will avoid.

            if (File.Exists(textfilenameFinal))
            {
                textfilenameFinal = textfilenameFinal.Replace(".txt", string.Format(".txt.(duplicate).{0:yyyyMMdd.HHmmss}.(PdfPig).txt", DateTime.Now));
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
