using System;
using System.Collections.Generic;

namespace Chien.EntityFrameworkCore.Toeic;

public class ToeicExam
{
    public int ExamId { get; set; }
    public string ExamCode { get; set; } = "";
    public string ExamTitle { get; set; } = "";
    public string? Description { get; set; }
    public short TimeLimitMinutes { get; set; }
    public short TotalQuestions { get; set; }
    public string Status { get; set; } = "DRAFT";
    public string? ImportedFileName { get; set; }
    public DateTime? ImportedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public ICollection<ToeicExamPart> Parts { get; set; } = new List<ToeicExamPart>();
}

public class ToeicExamPart
{
    public int PartId { get; set; }
    public int ExamId { get; set; }
    public byte PartNumber { get; set; }
    public string PartName { get; set; } = "";
    public string PartType { get; set; } = "";
    public byte DisplayOrder { get; set; }
    public ToeicExam Exam { get; set; } = null!;
    public ICollection<ToeicPassage> Passages { get; set; } = new List<ToeicPassage>();
    public ICollection<ToeicQuestion> Questions { get; set; } = new List<ToeicQuestion>();
}

public class ToeicPassage
{
    public int PassageId { get; set; }
    public int PartId { get; set; }
    public string PassageCode { get; set; } = "";
    public string? PassageTitle { get; set; }
    public string? PassageText { get; set; }
    public int DisplayOrder { get; set; }
    public ToeicExamPart Part { get; set; } = null!;
}

public class ToeicQuestion
{
    public int QuestionId { get; set; }
    public int ExamId { get; set; }
    public int PartId { get; set; }
    public int? PassageId { get; set; }
    public short QuestionNumber { get; set; }
    public string QuestionText { get; set; } = "";
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; }
    public ToeicExam Exam { get; set; } = null!;
    public ToeicExamPart Part { get; set; } = null!;
    public ICollection<ToeicOption> Options { get; set; } = new List<ToeicOption>();
}

public class ToeicOption
{
    public int OptionId { get; set; }
    public int QuestionId { get; set; }
    public string OptionKey { get; set; } = "";
    public string OptionText { get; set; } = "";
    public bool IsCorrect { get; set; }
    public byte DisplayOrder { get; set; }
    public ToeicQuestion Question { get; set; } = null!;
}
