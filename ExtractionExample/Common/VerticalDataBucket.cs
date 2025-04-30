using Cysharp.Text;
using UglyToad.PdfPig.DocumentLayoutAnalysis;

namespace ExtractionExample
{
    /// <summary>
    /// In the cases where the original PDF generator has saved the text you wanted, in columns instead of left-to-right
    /// </summary>
    public class VerticalDataBucket
    {
        private List<IReadOnlyList<TextLine>> _columns = new List<IReadOnlyList<TextLine>>();

        /// <summary>
        /// sequentially add
        /// </summary>
        public void Add(IReadOnlyList<TextLine> columns)
        {
            _columns.Add(columns);
        }

        public string Dump()
        {
            // Loop through the blocks that we have, and re-construct a line.
            // use PIPE as a delimiter, for simplicity

            int total_lines_max = 0;
            Utf16ValueStringBuilder output = ZString.CreateStringBuilder();

            foreach (IReadOnlyList<TextLine> column in _columns)
            {
                if (column.Count > total_lines_max) total_lines_max = column.Count; // find the max ..
            }

            for (int i = 0; i < total_lines_max; i++)
            {
                foreach (IReadOnlyList<TextLine> column in _columns)
                {
                    output.Append($"{column[i]}    |    ");
                }

                output.Append(Environment.NewLine);
            }

            output.Append(Environment.NewLine);
            output.Append(Environment.NewLine);

            return output.ToString();
        }
    }
}
