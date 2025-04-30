using DelightfulCode;
using System.Text.RegularExpressions;
using UglyToad.PdfPig.DocumentLayoutAnalysis;

public static partial class Extensions
{
    /// <summary>
    /// Eliminates a text block based on most likely conditions.
    /// WORKS For PyPDF2 files. (JPM)
    /// </summary>
    public static bool IsDataCandidate(this TextBlock segment)
    {
        // assume the text block has the data we want, unless we invalidate it early.

        Regex page_blah_of_blah = new Regex(@"Page \d+ of \d+");

        // TEST #1: Simple text matching

        if (segment.TextLines.Count == 1 && segment.Text.StartsWith("Brand")) return false;
        if (segment.TextLines.Count == 1 && segment.Text.StartsWith("Running Total")) return false;
        if (segment.TextLines.Count == 1 && segment.Text.StartsWith("Shape")) return false;
        if (segment.TextLines.Count == 1 && segment.Text.StartsWith("Bar No.")) return false;
        if (segment.TextLines.Count == 1 && segment.Text.StartsWith("Printed On: ")) return false;
        if (segment.TextLines.Count == 1 && segment.Text.StartsWith("Assay")) return false;
        if (segment.TextLines.Count == 1 && segment.Text.StartsWith("Gross Ounces")) return false;
        if (segment.TextLines.Count == 1 && segment.Text.StartsWith("Fine Ounces")) return false;
        if (segment.TextLines.Count == 1 && segment.Text.StartsWith("Vault")) return false;

        // TEST #2: regular expressions

        if (segment.TextLines.Count == 1 && page_blah_of_blah.IsMatch(segment.Text)) return false;

        // TEST #3: specific transforms

        if (segment.TextLines.Count == 1 && segment.Text.Replace(",", string.Empty).Replace(".", string.Empty).IsNumeric()) return false;

        // If all tests have not been invalidated, then it can be included.

        return true;
    }
}
