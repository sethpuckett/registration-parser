using System;
using System.Linq;
using System.Text;

namespace VoteParser
{
    using System.Collections.Generic;
    using System.Configuration;
    using System.IO;
    using System.Text.RegularExpressions;

    using iTextSharp.text.pdf;
    using iTextSharp.text.pdf.parser;

    public class RegistrationParser
    {
        private const string RowRegex = @"(^[^\d]*)(.{5}.*?\d{5})(\s\d\s\d\s-\s\w\s\w\s\w\s-\s\d\s\d\s)(...)(\s\d{1,3})(\s\d)(\s[NEVA])?(\s[NEVA])?(\s[NEVA])?(\s[NEVA])?(\s[NEVA])?(\s[NEVA])?(\s\d\w\d\d)";

        private const string AddressRegex = @"(^.*?\s[NS][EW])(\s.+)?(\s\s\d{5}$)";

        static void Main(string[] args)
        {
            var text = new StringBuilder();
            var pdfReader = new PdfReader(ConfigurationManager.AppSettings["RegistrationFilePath"]);

            for (var page = 1; page <= pdfReader.NumberOfPages; page++)
            {
                ITextExtractionStrategy strategy = new SimpleTextExtractionStrategy();

                var currentText = PdfTextExtractor.GetTextFromPage(pdfReader, page, strategy);

                currentText = Encoding.UTF8.GetString(Encoding.Convert(Encoding.Default, Encoding.UTF8, Encoding.Default.GetBytes(currentText)));
                text.Append(currentText);
            }

            pdfReader.Close();

            var lines = text.ToString().Split(new[] { '\n' }, StringSplitOptions.None);
            var csvBuilderDict = new Dictionary<char, StringBuilder>();

            foreach (var line in lines)

            {
                try
                {
                    var match = Regex.Match(line, RowRegex);
                    if (match.Success)
                    {
                        var name = match.Groups[1].ToString();
                        var nameParts = name.Split(',');
                        var lastName = nameParts[0].Trim().Replace(",", string.Empty);
                        var firstAndMidName = nameParts[1].Trim().Split(' ');
                        var firstName = firstAndMidName[0].Replace(",", string.Empty);
                        var midName = firstAndMidName.Length > 1 ? firstAndMidName[1].Replace(",", string.Empty) : null;
                        var address = match.Groups[2].ToString().Trim().Replace(",", string.Empty);
                        var address1 = string.Empty;
                        var address2 = string.Empty;
                        var address3 = string.Empty;

                        var addressMatch = Regex.Match(address, AddressRegex);
                        if (addressMatch.Success)
                        {
                            address1 = addressMatch.Groups[1].ToString().Trim();
                            address2 = addressMatch.Groups[2].ToString().Trim();
                            address3 = addressMatch.Groups[3].ToString().Trim();
                        }
                        
                        var registrationDate = new string(match.Groups[3].ToString().Replace(" ", string.Empty).Reverse().ToArray()).Replace(",", string.Empty);
                        var affiliation = match.Groups[4].ToString().Trim().Replace(",", string.Empty);
                        var precinct = match.Groups[5].ToString().Trim().Replace(",", string.Empty);
                        var ward = match.Groups[6].ToString().Trim().Replace(",", string.Empty);
                        var vote0415 = match.Groups[7].ToString().Trim().Replace(",", string.Empty);
                        var vote1114 = match.Groups[8].ToString().Trim().Replace(",", string.Empty);
                        var vote0714 = match.Groups[9].ToString().Trim().Replace(",", string.Empty);
                        var vote0414 = match.Groups[10].ToString().Trim().Replace(",", string.Empty);
                        var vote0413 = match.Groups[11].ToString().Trim().Replace(",", string.Empty);
                        var vote1112 = match.Groups[12].ToString().Trim().Replace(",", string.Empty);
                        var smd = match.Groups[13].ToString().Trim().Replace(",", string.Empty);

                        if (!csvBuilderDict.ContainsKey(lastName.First()))
                        {
                            var newSb = new StringBuilder("LastName, FirstName, MiddleInitial, FullAddress, Address1, Address2, Address3, RegistrationDate, Affiliation, Precinct, Ward, Vote0415, Vote1114, Vote0714, Vote0414, Vote0413, Vote1112, SingleMemberDistrict");
                            newSb.Append(Environment.NewLine);
                            csvBuilderDict.Add(lastName.First(), newSb);
                        }

                        StringBuilder sb;
                        csvBuilderDict.TryGetValue(lastName.First(), out sb);
                        sb.AppendLine($"{lastName},{firstName},{midName},{address},{address1},{address2},{address3},{registrationDate},{affiliation},{precinct},{ward},{vote0415},{vote1114},{vote0714},{vote0414},{vote0413},{vote1112},{smd}");
                    }
                }
                catch (Exception e)
                {
                    // Ignore and move on
                }
            }

            foreach (var stringBuilderEntry in csvBuilderDict)
            {
                File.WriteAllText($"registration-{stringBuilderEntry.Key}.csv", stringBuilderEntry.Value.ToString());
            }
        }
    }
}
