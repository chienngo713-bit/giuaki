using Chien.Controllers;
using Chien.EntityFrameworkCore.Toeic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Chien.Web.Toeic;

[Route("api/toeic/exams")]
public sealed class ToeicExamController : ChienControllerBase
{
    private readonly ToeicDbContextFactory _factory;
    private readonly ToeicTaggedWordParser _parser = new();

    public ToeicExamController(ToeicDbContextFactory factory) { _factory = factory; }

    [HttpPost("validate")]
    [RequestSizeLimit(50_000_000)]
    public ActionResult<ToeicValidationResult> Validate([FromForm(Name = "file")] IFormFile file)
    {
        if (file == null || file.Length == 0) return BadRequest(new { message = "Vui lòng chọn file DOCX." });
        if (!Path.GetExtension(file.FileName).Equals(".docx", StringComparison.OrdinalIgnoreCase)) return BadRequest(new { message = "Chỉ chấp nhận file .docx." });
        using var stream = file.OpenReadStream();
        return Ok(_parser.Parse(stream));
    }

    [HttpPost("import")]
    [RequestSizeLimit(50_000_000)]
    public async Task<ActionResult> Import([FromForm(Name = "file")] IFormFile file)
    {
        if (file == null || file.Length == 0) return BadRequest(new { message = "Vui lòng chọn file DOCX." });
        using var stream = file.OpenReadStream();
        var parsed = _parser.Parse(stream);
        if (!parsed.IsValid) return BadRequest(parsed);
        await using var db = _factory.Create();
        var code = "TOEIC-" + DateTime.UtcNow.ToString("yyyyMMddHHmmss");
        var exam = new ToeicExam { ExamCode = code, ExamTitle = parsed.Preview!.Title, TimeLimitMinutes = (short)parsed.Preview.TimeLimitMinutes, TotalQuestions = (short)parsed.Preview.TotalQuestions, Status = "DRAFT", ImportedFileName = file.FileName, ImportedAt = DateTime.UtcNow, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        foreach (var partNumber in new[] { 5, 6, 7 }) exam.Parts.Add(new ToeicExamPart { PartNumber = (byte)partNumber, PartName = "Part " + partNumber, PartType = partNumber == 5 ? "SINGLE" : "PASSAGE", DisplayOrder = (byte)(partNumber - 4) });
        db.Exams.Add(exam);
        await db.SaveChangesAsync();
        foreach (var q in parsed.Preview.Questions.OrderBy(x => x.Number))
        {
            var part = exam.Parts.Single(x => x.PartNumber == q.Part);
            var entity = new ToeicQuestion { ExamId = exam.ExamId, PartId = part.PartId, QuestionNumber = (short)q.Number, QuestionText = q.Text, DisplayOrder = q.Number, IsActive = true };
            foreach (var option in q.Options.OrderBy(x => x.Key)) entity.Options.Add(new ToeicOption { OptionKey = option.Key, OptionText = option.Value, IsCorrect = option.Key == q.Answer, DisplayOrder = (byte)(option.Key[0] - 'A' + 1) });
            db.Questions.Add(entity);
        }
        await db.SaveChangesAsync();
        return Ok(new { exam.ExamId, exam.ExamCode, message = "Import thành công." });
    }

    [HttpGet]
    public async Task<ActionResult> List()
    {
        await using var db = _factory.Create();
        if (!await db.Exams.AnyAsync())
        {
            var candidates = new[]
            {
                Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "assets", "TOEIC-Reading-Tagged.docx"),
                Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "TOEIC-Reading-Tagged.docx"),
                @"C:\CHIENIT\thigk\backend\assets\TOEIC-Reading-Tagged.docx",
                @"C:\CHIENIT\thigk\TOEIC-Reading-Tagged.docx"
            };
            var filePath = candidates.Select(Path.GetFullPath).FirstOrDefault(System.IO.File.Exists);
            if (filePath != null)
            {
                await using var fileStream = System.IO.File.OpenRead(filePath);
                var parsed = _parser.Parse(fileStream);
                if (parsed.IsValid && parsed.Preview != null)
                {
                    var now = DateTime.UtcNow;
                    var exam = new ToeicExam { ExamCode = "TOEIC-DEMO-" + now.ToString("yyyyMMddHHmmss"), ExamTitle = parsed.Preview.Title, TimeLimitMinutes = (short)parsed.Preview.TimeLimitMinutes, TotalQuestions = (short)parsed.Preview.TotalQuestions, Status = "DRAFT", ImportedFileName = Path.GetFileName(filePath), ImportedAt = now, CreatedAt = now, UpdatedAt = now };
                    foreach (var partNumber in new[] { 5, 6, 7 }) exam.Parts.Add(new ToeicExamPart { PartNumber = (byte)partNumber, PartName = "Part " + partNumber, PartType = partNumber == 5 ? "SINGLE" : "PASSAGE", DisplayOrder = (byte)(partNumber - 4) });
                    db.Exams.Add(exam);
                    await db.SaveChangesAsync();
                    foreach (var q in parsed.Preview.Questions.OrderBy(x => x.Number))
                    {
                        var part = exam.Parts.Single(x => x.PartNumber == q.Part);
                        var entity = new ToeicQuestion { ExamId = exam.ExamId, PartId = part.PartId, QuestionNumber = (short)q.Number, QuestionText = q.Text, DisplayOrder = q.Number, IsActive = true };
                        foreach (var option in q.Options.OrderBy(x => x.Key)) entity.Options.Add(new ToeicOption { OptionKey = option.Key, OptionText = option.Value, IsCorrect = option.Key == q.Answer, DisplayOrder = (byte)(option.Key[0] - 'A' + 1) });
                        db.Questions.Add(entity);
                    }
                    await db.SaveChangesAsync();
                }
            }
        }
        return Ok(await db.Exams.AsNoTracking().OrderByDescending(x => x.CreatedAt).Select(x => new { x.ExamId, x.ExamCode, x.ExamTitle, x.Status, x.TimeLimitMinutes, x.TotalQuestions, x.CreatedAt }).ToListAsync());
    }

    [HttpGet("{examId:int}/preview")]
    public async Task<ActionResult> Preview(int examId)
    {
        await using var db = _factory.Create();
        var exam = await db.Exams.AsNoTracking().Include(x => x.Parts).ThenInclude(x => x.Questions).ThenInclude(x => x.Options).SingleOrDefaultAsync(x => x.ExamId == examId);
        if (exam == null) return NotFound();
        return Ok(new { exam.ExamId, exam.ExamTitle, exam.Status, exam.TimeLimitMinutes, parts = exam.Parts.OrderBy(x => x.PartNumber).Select(p => new { p.PartNumber, questions = p.Questions.OrderBy(q => q.QuestionNumber).Select(q => new { q.QuestionId, q.QuestionNumber, q.QuestionText, options = q.Options.OrderBy(o => o.DisplayOrder).Select(o => new { o.OptionId, o.OptionKey, o.OptionText, o.IsCorrect }) }) }) });
    }

    [HttpGet("{examId:int}/questions")]
    public async Task<ActionResult> Questions(int examId)
    {
        await using var db = _factory.Create();
        var exam = await db.Exams.AsNoTracking()
            .Include(x => x.Parts).ThenInclude(x => x.Questions).ThenInclude(x => x.Options)
            .SingleOrDefaultAsync(x => x.ExamId == examId);
        if (exam == null) return NotFound();

        return Ok(new
        {
            exam.ExamId,
            exam.ExamTitle,
            exam.TimeLimitMinutes,
            exam.TotalQuestions,
            parts = exam.Parts.OrderBy(x => x.PartNumber).Select(p => new
            {
                p.PartNumber,
                questions = p.Questions.Where(q => q.IsActive).OrderBy(q => q.QuestionNumber).Select(q => new
                {
                    q.QuestionId,
                    q.QuestionNumber,
                    q.QuestionText,
                    options = q.Options.OrderBy(o => o.DisplayOrder).Select(o => new { o.OptionKey, o.OptionText })
                })
            })
        });
    }

    [HttpPost("{examId:int}/submit")]
    public async Task<ActionResult> Submit(int examId, [FromBody] ToeicSubmitInput input)
    {
        await using var db = _factory.Create();
        var questions = await db.Questions.AsNoTracking().Include(x => x.Options)
            .Where(x => x.ExamId == examId && x.IsActive).OrderBy(x => x.QuestionNumber).ToListAsync();
        if (questions.Count == 0) return NotFound();

        var answers = input?.Answers ?? new Dictionary<int, string>();
        var correct = questions.Count(q => answers.TryGetValue(q.QuestionId, out var answer)
            && q.Options.Any(o => o.IsCorrect && string.Equals(o.OptionKey, answer, StringComparison.OrdinalIgnoreCase)));
        var answered = questions.Count(q => answers.ContainsKey(q.QuestionId));

        return Ok(new
        {
            TotalQuestions = questions.Count,
            AnsweredQuestions = answered,
            UnansweredQuestions = questions.Count - answered,
            CorrectAnswers = correct,
            WrongAnswers = questions.Count - correct,
            Score = (int)Math.Round(correct * 100d / questions.Count, MidpointRounding.AwayFromZero)
        });
    }

    [HttpPut("questions/{questionId:int}")]
    public async Task<ActionResult> UpdateQuestion(int questionId, [FromBody] ToeicQuestionUpdate input)
    {
        if (input == null || string.IsNullOrWhiteSpace(input.Text) || input.Options == null || input.Options.Count != 4 || input.Answer == null || !new[] { "A", "B", "C", "D" }.Contains(input.Answer)) return BadRequest(new { message = "Nội dung, đủ 4 đáp án và đáp án đúng là bắt buộc." });
        await using var db = _factory.Create();
        var q = await db.Questions.Include(x => x.Options).SingleOrDefaultAsync(x => x.QuestionId == questionId);
        if (q == null) return NotFound();
        q.QuestionText = input.Text.Trim();
        foreach (var option in q.Options) { if (!input.Options.TryGetValue(option.OptionKey, out var text) || string.IsNullOrWhiteSpace(text)) return BadRequest(new { message = "Thiếu nội dung đáp án " + option.OptionKey }); option.OptionText = text.Trim(); option.IsCorrect = option.OptionKey == input.Answer; }
        await db.SaveChangesAsync();
        return Ok(new { message = "Đã cập nhật câu hỏi." });
    }
}
