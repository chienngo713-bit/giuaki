# Sơ đồ cơ sở dữ liệu TOEIC Reading

Sơ đồ dưới đây có thể chèn vào báo cáo hoặc dùng để dựng lại trên draw.io, dbdiagram.io hay SSMS Database Diagrams.

```mermaid
erDiagram
    APP_USER ||--o{ EXAM : imports
    APP_USER ||--o{ EXAM_IMPORT : performs
    APP_USER ||--o{ EXAM_ATTEMPT : takes
    EXAM ||--o{ EXAM_PART : contains
    EXAM ||--o{ QUESTION : owns
    EXAM ||--o{ EXAM_IMPORT : has
    EXAM_PART ||--o{ READING_PASSAGE : contains
    EXAM_PART ||--o{ QUESTION : groups
    READING_PASSAGE ||--o{ QUESTION : contextualizes
    QUESTION ||--|{ QUESTION_OPTION : offers
    EXAM ||--o{ EXAM_ATTEMPT : attempted
    EXAM_ATTEMPT ||--o{ ATTEMPT_ANSWER : records
    QUESTION ||--o{ ATTEMPT_ANSWER : answered
    QUESTION_OPTION ||--o{ ATTEMPT_ANSWER : selected

    APP_USER {
        int UserId PK
        nvarchar UserName UK
        varchar RoleName
        bit IsActive
    }
    EXAM {
        int ExamId PK
        varchar ExamCode UK
        nvarchar ExamTitle
        smallint TimeLimitMinutes
        varchar Status
    }
    EXAM_PART {
        int PartId PK
        int ExamId FK
        tinyint PartNumber
        varchar PartType
    }
    READING_PASSAGE {
        int PassageId PK
        int PartId FK
        varchar PassageCode
        nvarchar PassageText
    }
    QUESTION {
        int QuestionId PK
        int ExamId FK
        int PartId FK
        int PassageId FK
        smallint QuestionNumber
        nvarchar QuestionText
    }
    QUESTION_OPTION {
        int OptionId PK
        int QuestionId FK
        char OptionKey
        nvarchar OptionText
        bit IsCorrect
    }
    EXAM_IMPORT {
        bigint ImportId PK
        int ExamId FK
        int ImportedBy FK
        varchar ImportStatus
    }
    EXAM_ATTEMPT {
        bigint AttemptId PK
        int ExamId FK
        int UserId FK
        varchar AttemptStatus
        decimal Score
    }
    ATTEMPT_ANSWER {
        bigint AttemptAnswerId PK
        bigint AttemptId FK
        int QuestionId FK
        int SelectedOptionId FK
        bit IsCorrect
    }
```

## Ý nghĩa chính

- `Exam`, `ExamPart`, `ReadingPassage`, `Question`, `QuestionOption`: lưu nội dung đề theo đúng cấu trúc Part 5, 6 và 7.
- `ExamImport`: lưu lịch sử import và lỗi validation; chỉ tạo `Exam` hoàn chỉnh khi file Word hợp lệ.
- `ExamAttempt`, `AttemptAnswer`: lưu bài làm tạm thời, bài đã nộp và kết quả chấm.
- `IsCorrect` chỉ được dùng ở dữ liệu đáp án của đề; `IsCorrect` trong `AttemptAnswer` là kết quả chấm tại thời điểm nộp bài.
