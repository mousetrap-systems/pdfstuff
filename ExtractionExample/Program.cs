using DelightfulCode;
using Spectre.Console;

namespace ExtractionExample
{
    internal class Program
    {
        /// <summary>
        /// Generic demonstration of PDF extraction suitable for large files, using two different libraries.
        /// </summary>
        [Author("Warren James", 2025)]
        static void Main(string[] args)
        {
            AnsiConsole.Write(new FigletText("PDF Extraction").LeftJustified().Color(Color.Gold3));

            DirectoryInfo di = new DirectoryInfo(@"D:\BULLIONBARS\_STEP1_DownloadsBucket");

            List<FileInfo> files = di.GetFiles($"*.pdf").OrderByDescending(a => a.LastWriteTimeUtc).Take(20).ToList(); // change this to whatever ..
            List<string> choices = files.Select(f => f.Name).ToList();

            // TODO: extend core selections of files, this can obviously be more practical

            if (files.Count == 0)
            {
                AnsiConsole.MarkupLine($"[gray]No PDF files were found in {di.FullName}.[/]");
            }
            else
            {
                string selected = AnsiConsole.Prompt(new SelectionPrompt<string>().Title($"Select a PDF file:").PageSize(choices.Count).AddChoices(choices));

                if (String.IsNullOrEmpty(selected))
                {
                    AnsiConsole.MarkupLine("[gray]No file selected, no processing done.[/]");
                }
                else
                {
                    FileInfo? selectedFile = files.FirstOrDefault(file => file.Name == selected);

                    if (selectedFile != null)
                    {
                        // MAIN PDF extraction WORK!

                        FileInfo? result1 = PDFPIG.ExtractText(selectedFile.FullName, reportProgress: true, useVerticalBlocksLayout: false);
                        FileInfo? result2 = PDFiText7.ExtractText(selectedFile.FullName, reportProgress: true);

                        // Now display the results of that selection

                        Table table = new Table()
                            .Border(TableBorder.Rounded)
                            .BorderColor(Color.LightSlateGrey);

                        table.AddColumn(new TableColumn($"[bold Gold3]Source[/]"));
                        table.AddColumn(new TableColumn($"[bold Gold3]{selectedFile.Directory}[/]"));
                        table.AddColumn(new TableColumn($"[bold Gold3]Size (bytes)[/]"));

                        table.AddRow(
                            "[gray]Original[/]",
                            $"[gray]{selectedFile.Name}[/]",
                            selectedFile.Exists ? $"[gray]{selectedFile.Length.ToString()}[/]" : "[red]N/A[/]"
                        );

                        table.AddRow(
                            "[magenta]PDFPIG[/]",
                            result1?.Name != null ? "[#8B008B]" + result1.Name + "[/]" : "[grey]N/A[/]",
                            (result1 != null && result1.Exists) ? result1.Length.ToString() : "[red]N/A[/]"
                        );

                        table.AddRow(
                            "[blue]PDFiText7[/]",
                            result2?.Name != null ? "[#104E8B]" + result2.Name + "[/]" : "[grey]N/A[/]",
                            (result2 != null && result2.Exists) ? result2.Length.ToString() : "[red]N/A[/]"
                        );

                        AnsiConsole.Write(table);
                    }
                }
            }
        }

    }
}
