/*
  TOEIC Reading Exam System - SQL Server schema
  Compatible with SQL Server 2019+
*/

IF DB_ID(N'TOEICExamDb') IS NULL
    CREATE DATABASE TOEICExamDb;
GO

USE TOEICExamDb;
GO

/* Users */
IF OBJECT_ID(N'dbo.AppUser', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.AppUser
    (
        UserId          INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_AppUser PRIMARY KEY,
        UserName        NVARCHAR(100) NOT NULL CONSTRAINT UQ_AppUser_UserName UNIQUE,
        Email           NVARCHAR(255) NULL CONSTRAINT UQ_AppUser_Email UNIQUE,
        PasswordHash    NVARCHAR(500) NULL,
        DisplayName     NVARCHAR(200) NULL,
        RoleName        VARCHAR(20) NOT NULL CONSTRAINT DF_AppUser_Role DEFAULT ('USER'),
        IsActive        BIT NOT NULL CONSTRAINT DF_AppUser_IsActive DEFAULT (1),
        CreatedAt       DATETIME2(0) NOT NULL CONSTRAINT DF_AppUser_CreatedAt DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT CK_AppUser_Role CHECK (RoleName IN ('ADMIN','USER'))
    );
END;
GO

/* Exam and its hierarchical content */
IF OBJECT_ID(N'dbo.Exam', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Exam
    (
        ExamId          INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Exam PRIMARY KEY,
        ExamCode        VARCHAR(50) NOT NULL CONSTRAINT UQ_Exam_ExamCode UNIQUE,
        ExamTitle       NVARCHAR(255) NOT NULL,
        Description     NVARCHAR(MAX) NULL,
        TimeLimitMinutes SMALLINT NOT NULL CONSTRAINT DF_Exam_TimeLimit DEFAULT (75),
        TotalQuestions  SMALLINT NOT NULL CONSTRAINT DF_Exam_TotalQuestions DEFAULT (100),
        Status          VARCHAR(20) NOT NULL CONSTRAINT DF_Exam_Status DEFAULT ('DRAFT'),
        ImportedFileName NVARCHAR(255) NULL,
        ImportedBy      INT NULL,
        ImportedAt      DATETIME2(0) NULL,
        PublishedAt     DATETIME2(0) NULL,
        CreatedAt       DATETIME2(0) NOT NULL CONSTRAINT DF_Exam_CreatedAt DEFAULT (SYSUTCDATETIME()),
        UpdatedAt       DATETIME2(0) NOT NULL CONSTRAINT DF_Exam_UpdatedAt DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT CK_Exam_TimeLimit CHECK (TimeLimitMinutes > 0),
        CONSTRAINT CK_Exam_TotalQuestions CHECK (TotalQuestions > 0),
        CONSTRAINT CK_Exam_Status CHECK (Status IN ('DRAFT','PUBLISHED','ARCHIVED')),
        CONSTRAINT FK_Exam_ImportedBy FOREIGN KEY (ImportedBy) REFERENCES dbo.AppUser(UserId)
    );
END;
GO

IF OBJECT_ID(N'dbo.ExamPart', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.ExamPart
    (
        PartId          INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_ExamPart PRIMARY KEY,
        ExamId          INT NOT NULL,
        PartNumber      TINYINT NOT NULL,
        PartName        NVARCHAR(100) NOT NULL,
        PartType        VARCHAR(20) NOT NULL,
        DisplayOrder    TINYINT NOT NULL,
        CONSTRAINT UQ_ExamPart_Exam_Part UNIQUE (ExamId, PartNumber),
        CONSTRAINT CK_ExamPart_Number CHECK (PartNumber IN (5,6,7)),
        CONSTRAINT CK_ExamPart_Type CHECK (PartType IN ('SINGLE','PASSAGE')),
        CONSTRAINT FK_ExamPart_Exam FOREIGN KEY (ExamId) REFERENCES dbo.Exam(ExamId) ON DELETE CASCADE
    );
END;
GO

IF OBJECT_ID(N'dbo.ReadingPassage', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.ReadingPassage
    (
        PassageId       INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_ReadingPassage PRIMARY KEY,
        PartId          INT NOT NULL,
        PassageCode     VARCHAR(50) NOT NULL,
        PassageTitle    NVARCHAR(500) NULL,
        PassageText     NVARCHAR(MAX) NULL,
        PassageImageUrl NVARCHAR(1000) NULL,
        DisplayOrder    INT NOT NULL CONSTRAINT DF_ReadingPassage_DisplayOrder DEFAULT (1),
        CONSTRAINT UQ_ReadingPassage_Part_Code UNIQUE (PartId, PassageCode),
        CONSTRAINT FK_ReadingPassage_Part FOREIGN KEY (PartId) REFERENCES dbo.ExamPart(PartId) ON DELETE CASCADE
    );
END;
GO

IF OBJECT_ID(N'dbo.Question', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Question
    (
        QuestionId      INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Question PRIMARY KEY,
        ExamId          INT NOT NULL,
        PartId          INT NOT NULL,
        PassageId       INT NULL,
        QuestionNumber  SMALLINT NOT NULL,
        QuestionText    NVARCHAR(MAX) NOT NULL,
        Explanation     NVARCHAR(MAX) NULL,
        DisplayOrder    INT NOT NULL,
        IsActive        BIT NOT NULL CONSTRAINT DF_Question_IsActive DEFAULT (1),
        CONSTRAINT UQ_Question_Exam_Number UNIQUE (ExamId, QuestionNumber),
        CONSTRAINT FK_Question_Exam FOREIGN KEY (ExamId) REFERENCES dbo.Exam(ExamId) ON DELETE CASCADE,
        CONSTRAINT FK_Question_Part FOREIGN KEY (PartId) REFERENCES dbo.ExamPart(PartId),
        CONSTRAINT FK_Question_Passage FOREIGN KEY (PassageId) REFERENCES dbo.ReadingPassage(PassageId)
    );
END;
GO

IF OBJECT_ID(N'dbo.QuestionOption', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.QuestionOption
    (
        OptionId        INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_QuestionOption PRIMARY KEY,
        QuestionId      INT NOT NULL,
        OptionKey       CHAR(1) NOT NULL,
        OptionText      NVARCHAR(MAX) NOT NULL,
        IsCorrect       BIT NOT NULL CONSTRAINT DF_QuestionOption_IsCorrect DEFAULT (0),
        DisplayOrder    TINYINT NOT NULL,
        CONSTRAINT UQ_QuestionOption_Key UNIQUE (QuestionId, OptionKey),
        CONSTRAINT CK_QuestionOption_Key CHECK (OptionKey IN ('A','B','C','D')),
        CONSTRAINT FK_QuestionOption_Question FOREIGN KEY (QuestionId) REFERENCES dbo.Question(QuestionId) ON DELETE CASCADE
    );
END;
GO

/* Import history and validation errors */
IF OBJECT_ID(N'dbo.ExamImport', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.ExamImport
    (
        ImportId        BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_ExamImport PRIMARY KEY,
        ExamId          INT NULL,
        FileName        NVARCHAR(255) NOT NULL,
        FileHash        VARCHAR(128) NULL,
        ImportedBy      INT NOT NULL,
        ImportStatus    VARCHAR(20) NOT NULL,
        ErrorMessage    NVARCHAR(MAX) NULL,
        ImportedAt      DATETIME2(0) NOT NULL CONSTRAINT DF_ExamImport_ImportedAt DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT CK_ExamImport_Status CHECK (ImportStatus IN ('PROCESSING','SUCCESS','FAILED')),
        CONSTRAINT FK_ExamImport_Exam FOREIGN KEY (ExamId) REFERENCES dbo.Exam(ExamId),
        CONSTRAINT FK_ExamImport_User FOREIGN KEY (ImportedBy) REFERENCES dbo.AppUser(UserId)
    );
END;
GO

/* User attempts, saved answers, and final scoring */
IF OBJECT_ID(N'dbo.ExamAttempt', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.ExamAttempt
    (
        AttemptId       BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_ExamAttempt PRIMARY KEY,
        ExamId          INT NOT NULL,
        UserId          INT NOT NULL,
        StartedAt       DATETIME2(0) NOT NULL CONSTRAINT DF_ExamAttempt_StartedAt DEFAULT (SYSUTCDATETIME()),
        SubmittedAt     DATETIME2(0) NULL,
        AttemptStatus   VARCHAR(20) NOT NULL CONSTRAINT DF_ExamAttempt_Status DEFAULT ('IN_PROGRESS'),
        CorrectCount    SMALLINT NULL,
        WrongCount      SMALLINT NULL,
        SkippedCount    SMALLINT NULL,
        Score           DECIMAL(6,2) NULL,
        CONSTRAINT CK_ExamAttempt_Status CHECK (AttemptStatus IN ('IN_PROGRESS','SUBMITTED','TIMEOUT','CANCELLED')),
        CONSTRAINT FK_ExamAttempt_Exam FOREIGN KEY (ExamId) REFERENCES dbo.Exam(ExamId),
        CONSTRAINT FK_ExamAttempt_User FOREIGN KEY (UserId) REFERENCES dbo.AppUser(UserId)
    );
END;
GO

IF OBJECT_ID(N'dbo.AttemptAnswer', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.AttemptAnswer
    (
        AttemptAnswerId BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_AttemptAnswer PRIMARY KEY,
        AttemptId       BIGINT NOT NULL,
        QuestionId      INT NOT NULL,
        SelectedOptionId INT NULL,
        AnsweredAt      DATETIME2(0) NOT NULL CONSTRAINT DF_AttemptAnswer_AnsweredAt DEFAULT (SYSUTCDATETIME()),
        IsCorrect       BIT NULL,
        CONSTRAINT UQ_AttemptAnswer_Attempt_Question UNIQUE (AttemptId, QuestionId),
        CONSTRAINT FK_AttemptAnswer_Attempt FOREIGN KEY (AttemptId) REFERENCES dbo.ExamAttempt(AttemptId) ON DELETE CASCADE,
        CONSTRAINT FK_AttemptAnswer_Question FOREIGN KEY (QuestionId) REFERENCES dbo.Question(QuestionId),
        CONSTRAINT FK_AttemptAnswer_Option FOREIGN KEY (SelectedOptionId) REFERENCES dbo.QuestionOption(OptionId)
    );
END;
GO

CREATE INDEX IX_Exam_Status ON dbo.Exam(Status, CreatedAt DESC);
CREATE INDEX IX_Question_Exam_Order ON dbo.Question(ExamId, DisplayOrder);
CREATE INDEX IX_Question_Passage_Order ON dbo.Question(PassageId, DisplayOrder);
CREATE INDEX IX_ExamAttempt_User_Status ON dbo.ExamAttempt(UserId, AttemptStatus, StartedAt DESC);
GO

/* Read-only view for the mobile exam screen */
CREATE OR ALTER VIEW dbo.vw_ExamQuestion
AS
SELECT
    e.ExamId, e.ExamCode, e.ExamTitle, e.TimeLimitMinutes,
    p.PartNumber, p.PartName, p.PartType,
    rp.PassageId, rp.PassageCode, rp.PassageTitle, rp.PassageText,
    q.QuestionId, q.QuestionNumber, q.QuestionText, q.DisplayOrder,
    o.OptionId, o.OptionKey, o.OptionText, o.DisplayOrder AS OptionOrder
FROM dbo.Exam e
JOIN dbo.ExamPart p ON p.ExamId = e.ExamId
JOIN dbo.Question q ON q.ExamId = e.ExamId AND q.PartId = p.PartId
LEFT JOIN dbo.ReadingPassage rp ON rp.PassageId = q.PassageId
JOIN dbo.QuestionOption o ON o.QuestionId = q.QuestionId
WHERE e.Status = 'PUBLISHED' AND q.IsActive = 1;
GO

/* Basic integrity checks used after importing a Word file */
CREATE OR ALTER PROCEDURE dbo.ValidateExamStructure
    @ExamId INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT q.QuestionNumber, COUNT(o.OptionId) AS OptionCount,
           SUM(CASE WHEN o.IsCorrect = 1 THEN 1 ELSE 0 END) AS CorrectOptionCount
    FROM dbo.Question q
    LEFT JOIN dbo.QuestionOption o ON o.QuestionId = q.QuestionId
    WHERE q.ExamId = @ExamId
    GROUP BY q.QuestionNumber
    HAVING COUNT(o.OptionId) <> 4
        OR SUM(CASE WHEN o.IsCorrect = 1 THEN 1 ELSE 0 END) <> 1;
END;
GO
