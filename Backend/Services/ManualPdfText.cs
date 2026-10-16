using System.Text;
using UglyToad.PdfPig;

namespace Backend.Services;

public static class ManualPdfText
{
    public static string Read(Stream stream)
    {
        using var document = PdfDocument.Open(stream);
        var builder = new StringBuilder();
        foreach (var page in document.GetPages())
        {
            var words = page.GetWords().ToList();
            if (words.Count == 0)
            {
                builder.AppendLine(page.Text);
                continue;
            }

            var lines = words
                .GroupBy(word => Math.Round(word.BoundingBox.Bottom / 3d) * 3d)
                .OrderByDescending(group => group.Key);

            foreach (var line in lines)
            {
                var text = string.Join(
                    " ",
                    line.OrderBy(word => word.BoundingBox.Left).Select(word => word.Text));
                builder.AppendLine(text);
            }

            builder.AppendLine();
        }

        return builder.ToString();
    }
}
