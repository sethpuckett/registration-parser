using System;
using System.Linq;
using System.Text;

namespace VoteParser
{
    using System.Configuration;
    using System.Text.RegularExpressions;

    using iTextSharp.text.pdf;
    using iTextSharp.text.pdf.parser;

    public class RegistrationParser
    {
        private const string RowRegex = @"(^[^\d]*)(.{5}.*?\d\d\d\d\d)(\s\d\s\d\s-\s\w\s\w\s\w\s-\s\d\s\d\s)(...)(\s\d{1,3})(\s\d)(\s[NEV])?(\s[NEV])?(\s[NEV])?(\s[NEV])?(\s[NEV])?(\s[NEV])?(\s\d\w\d\d)";

        static void Main(string[] args)
        {
            StringBuilder text = new StringBuilder();
            PdfReader pdfReader = new PdfReader(ConfigurationManager.AppSettings["RegistrationFilePath"]);

            for (int page = 1; page <= 100 /* pdfReader.NumberOfPages */; page++)
            {
                ITextExtractionStrategy strategy = new SimpleTextExtractionStrategy();

                string currentText = PdfTextExtractor.GetTextFromPage(pdfReader, page, strategy);

                currentText = Encoding.UTF8.GetString(Encoding.Convert(Encoding.Default, Encoding.UTF8, Encoding.Default.GetBytes(currentText)));
                text.Append(currentText);
            }

            pdfReader.Close();

            var lines = text.ToString().Split(new[] { '\n' }, StringSplitOptions.None);

            foreach (var line in lines)
            {
                try
                {
                    var match = Regex.Match(line, RowRegex);
                    if (match.Success)
                    {
                        var name = match.Groups[1].ToString();
                        var nameParts = name.Split(',');
                        var lastName = nameParts[0].Trim();
                        var firstAndMidName = nameParts[1].Trim().Split(' ');
                        var firstName = firstAndMidName[0];
                        var midName = firstAndMidName.Length > 1 ? firstAndMidName[1] : null;
                        var address = match.Groups[2].ToString().Trim();
                        var registrationDate = new string(match.Groups[3].ToString().Replace(" ", string.Empty).Reverse().ToArray());
                        var affiliation = match.Groups[4].ToString().Trim();
                        var precinct = match.Groups[5].ToString().Trim();
                        var ward = match.Groups[6].ToString().Trim();
                        var vote0415 = match.Groups[7].ToString().Trim();
                        var vote1114 = match.Groups[8].ToString().Trim();
                        var vote0714 = match.Groups[9].ToString().Trim();
                        var vote0414 = match.Groups[10].ToString().Trim();
                        var vote0413 = match.Groups[11].ToString().Trim();
                        var vote1112 = match.Groups[12].ToString().Trim();
                        var smd = match.Groups[13].ToString().Trim();
                    }
                }
                catch (Exception)
                {
                    // Ignore and move on
                }
                
            }

        }
    }
}
