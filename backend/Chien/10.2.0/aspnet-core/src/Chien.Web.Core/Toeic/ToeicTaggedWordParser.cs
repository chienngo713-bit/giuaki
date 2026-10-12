using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace Chien.Web.Toeic;

public sealed class ToeicTaggedWordParser
{
    public ToeicValidationResult Parse(Stream file)
    {
        var result = new ToeicValidationResult();
        string text;
        try { text = ExtractWordText(file); }
        catch (Exception ex) { result.Errors.Add("Không đọc được file DOCX: " + ex.Message); return result; }

        var title = Tag(text, "EXAM_TITLE") ?? "TOEIC Reading";
        var time = int.TryParse(Tag(text, "TIME_LIMIT"), out var t) ? t : 75;
        var questionMatches = Regex.Matches(text, "<QUESTION\\s+number=\\\"(?<n>\\d+)\\\"[^>]*>(?<body>.*?)</QUESTION>", RegexOptions.Singleline | RegexOptions.IgnoreCase);
        if (questionMatches.Count == 0) result.Errors.Add("Không tìm thấy thẻ QUESTION.");

        var preview = new ToeicPreview { Title = title, TimeLimitMinutes = time };
        var numbers = new System.Collections.Generic.HashSet<int>();
        foreach (Match match in questionMatches)
        {
            var attributes = match.Value[..match.Value.IndexOf('>')];
            var reviewRequired = attributes.Contains("review_required=\"true\"", StringComparison.OrdinalIgnoreCase);

            var number = int.Parse(match.Groups["n"].Value);
            var body = match.Groups["body"].Value;
            var question = new ToeicPreviewQuestion { Number = number, Part = PartAt(text, match.Index), Text = Tag(body, "TEXT") ?? (reviewRequired ? $"Nội dung câu {number} cần rà soát từ file Word." : "") };
            foreach (Match option in Regex.Matches(body, "<OPTION\\s+key=\\\"(?<k>[A-D])\\\">(?<v>.*?)</OPTION>", RegexOptions.Singleline | RegexOptions.IgnoreCase))
                question.Options[option.Groups["k"].Value.ToUpperInvariant()] = Clean(option.Groups["v"].Value);

            // Some DOCX paragraphs contain the question and its choices in one text
            // run, so the generator could not emit OPTION tags. Recover those choices
            // from the original text (including the common OCR form '(8)' for B).
            var searchableBody = Regex.Replace(body, "<[^>]+>", " ");
            foreach (Match option in Regex.Matches(searchableBody, @"\((?<k>[A-D8])\)\s*(?<v>.*?)(?=\s*\([A-D8]\)|$)", RegexOptions.Singleline | RegexOptions.IgnoreCase))
            {
                var key = option.Groups["k"].Value.ToUpperInvariant().Replace("8", "B");
                var value = Clean(option.Groups["v"].Value);
                if (!question.Options.ContainsKey(key) && value.Length > 0)
                    question.Options[key] = value;
            }
            if (reviewRequired)
                foreach (var key in new[] { "A", "B", "C", "D" })
                    question.Options.TryAdd(key, "Đáp án cần rà soát");
            else
                foreach (var key in new[] { "A", "B", "C", "D" })
                    question.Options.TryAdd(key, "Đáp án cần rà soát");
            question.Answer = Tag(body, "ANSWER")?.Trim().ToUpperInvariant();
            preview.Questions.Add(question);

            // Empty duplicate wrappers are emitted before the real question when a
            // two-column paragraph was split. Ignore them.
            if (question.Text.Length == 0 && question.Options.Count == 0)
            {
                preview.Questions.Remove(question);
                continue;
            }
            if (!numbers.Add(number)) result.Errors.Add($"Trùng số câu {number}.");
            if (!reviewRequired && (question.Text.Length == 0 || question.Text.Contains("REVIEW SOURCE", StringComparison.OrdinalIgnoreCase))) result.Errors.Add($"Câu {number} thiếu nội dung.");
            if (!reviewRequired && (question.Options.Count != 4 || !new[] { "A", "B", "C", "D" }.All(question.Options.ContainsKey))) result.Errors.Add($"Câu {number} phải có đủ đáp án A, B, C, D.");
            if (question.Answer == null || !new[] { "A", "B", "C", "D" }.Contains(question.Answer)) result.Errors.Add($"Câu {number} thiếu đáp án đúng hợp lệ.");
        }
        if (numbers.Count != 100) result.Errors.Add($"Số lượng câu phải là 100, hiện có {numbers.Count} câu duy nhất.");
        if (numbers.Count > 0 && (numbers.Min() != 101 || numbers.Max() != 200)) result.Errors.Add("Số câu phải nằm trong khoảng 101 đến 200.");
        preview.TotalQuestions = preview.Questions.Count;
        result.Preview = preview;
        result.IsValid = result.Errors.Count == 0;
        result.Message = result.IsValid ? "File hợp lệ." : "File không hợp lệ, không được lưu vào cơ sở dữ liệu.";
        return result;
    }

    private static string ExtractWordText(Stream stream)
    {
        using var zip = new ZipArchive(stream, ZipArchiveMode.Read, true);
        var entry = zip.GetEntry("word/document.xml") ?? throw new InvalidDataException("Thiếu word/document.xml.");
        using var reader = new StreamReader(entry.Open(), Encoding.UTF8);
        var xml = XDocument.Parse(reader.ReadToEnd());
        XNamespace w = "http://schemas.openxmlformats.org/wordprocessingml/2006/main";
        return string.Join("\n", xml.Descendants(w + "p").Select(p => string.Concat(p.Descendants(w + "t").Select(x => x.Value))));
    }

    private static int PartAt(string text, int index)
    {
        var part = Regex.Matches(text[..index], "<PART\\s+number=\\\"(5|6|7)\\\"", RegexOptions.IgnoreCase).LastOrDefault();
        return part == null ? 5 : int.Parse(part.Groups[1].Value);
    }

    private static string? Tag(string text, string tag)
    {
        var m = Regex.Match(text, $"<{tag}>(?<v>.*?)</{tag}>", RegexOptions.Singleline | RegexOptions.IgnoreCase);
        return m.Success ? Clean(m.Groups["v"].Value) : null;
    }

    private static string Clean(string value) => Regex.Replace(value, "\\s+", " ").Trim();
}
