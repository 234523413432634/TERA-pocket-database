using System.Text;
using System.Text.RegularExpressions;

namespace MMOItemKnowledgeBase
{
    public static class HtmlToRtfConverter
    {
        public static string ConvertSimpleHtmlToRtf(string html)
        {
            if (string.IsNullOrEmpty(html))
                return string.Empty;

            // First convert custom tags to temporary HTML-like tags
            string processed = html
                .Replace("$BR", "<br>")
                .Replace("$H_W_BAD", "<bad>")
                .Replace("$H_W_GOOD", "<good>")
                .Replace("$COLOR_END", "</color>");

            var rtf = new StringBuilder(processed);

            // Replace line breaks
            rtf.Replace("<br>", "\\line ")
               .Replace("<br/>", "\\line ")
               .Replace("<br />", "\\line ");

            // Replace paragraphs
            rtf.Replace("<p>", "\\par ")
               .Replace("</p>", "\\par ");

            // Convert the StringBuilder to string for regex processing
            string processedText = rtf.ToString();

            // Process custom color tags
            processedText = Regex.Replace(processedText,
                @"<bad>(.*?)</color>",
                match => $"{{\\cf2 {match.Groups[1].Value}}}",
                RegexOptions.Singleline);

            processedText = Regex.Replace(processedText,
                @"<good>(.*?)</color>",
                match => $"{{\\cf3 {match.Groups[1].Value}}}",
                RegexOptions.Singleline);

            // Process standard HTML colors
            processedText = Regex.Replace(processedText,
                @"<font color=""#([0-9a-fA-F]{6})"">(.*?)</font>",
                match =>
                {
                    var color = match.Groups[1].Value;
                    var text = match.Groups[2].Value;
                    return $"{{\\cf1\\chshdng0\\chcbpat1\\cb1\\cf1\\highlight1\\cf1 {text}}}";
                },
                RegexOptions.Singleline);

            // Remove other HTML tags we don't support
            processedText = Regex.Replace(processedText, "<.*?>", string.Empty);

            // RTF header with color definitions:
            // \cf0 = default color (index 1 in color table)
            // \cf2 = red (index 2 in color table)
            // \cf3 = green (index 3 in color table)
            string header = @"{\rtf1\ansi\deff0{\colortbl;\red0\green0\blue0;\red255\green0\blue0;\red0\green255\blue0;\red0\green0\blue255;}";
            string footer = @"}";

            return header + processedText + footer;
        }
    }
}