using System.Collections.Generic;

namespace Chien.Web.Toeic;

public sealed class ToeicValidationResult
{
    public bool IsValid { get; set; }
    public string? Message { get; set; }
    public List<string> Errors { get; set; } = new();
    public ToeicPreview? Preview { get; set; }
}

public sealed class ToeicPreview
{
    public string Title { get; set; } = "";
    public int TimeLimitMinutes { get; set; }
    public int TotalQuestions { get; set; }
    public List<ToeicPreviewQuestion> Questions { get; set; } = new();
}

public sealed class ToeicPreviewQuestion
{
    public int Number { get; set; }
    public int Part { get; set; }
    public string Text { get; set; } = "";
    public Dictionary<string, string> Options { get; set; } = new();
    public string? Answer { get; set; }
}

public sealed class ToeicQuestionUpdate
{
    public string Text { get; set; } = "";
    public Dictionary<string, string> Options { get; set; } = new();
    public string? Answer { get; set; }
}

public sealed class ToeicSubmitInput
{
    public Dictionary<int, string> Answers { get; set; } = new();
}
