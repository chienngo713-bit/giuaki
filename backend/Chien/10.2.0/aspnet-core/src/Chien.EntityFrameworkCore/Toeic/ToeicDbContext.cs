using Microsoft.EntityFrameworkCore;

namespace Chien.EntityFrameworkCore.Toeic;

public class ToeicDbContext : DbContext
{
    public ToeicDbContext(DbContextOptions<ToeicDbContext> options) : base(options) { }
    public DbSet<ToeicExam> Exams => Set<ToeicExam>();
    public DbSet<ToeicExamPart> Parts => Set<ToeicExamPart>();
    public DbSet<ToeicPassage> Passages => Set<ToeicPassage>();
    public DbSet<ToeicQuestion> Questions => Set<ToeicQuestion>();
    public DbSet<ToeicOption> Options => Set<ToeicOption>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<ToeicExam>(e => { e.ToTable("Exam"); e.HasKey(x => x.ExamId); e.Property(x => x.ExamCode).HasMaxLength(50).IsRequired(); e.Property(x => x.ExamTitle).HasMaxLength(255).IsRequired(); e.Property(x => x.Status).HasMaxLength(20).IsRequired(); e.HasIndex(x => x.ExamCode).IsUnique(); });
        b.Entity<ToeicExamPart>(e => { e.ToTable("ExamPart"); e.HasKey(x => x.PartId); e.Property(x => x.PartName).HasMaxLength(100).IsRequired(); e.Property(x => x.PartType).HasMaxLength(20).IsRequired(); e.HasIndex(x => new { x.ExamId, x.PartNumber }).IsUnique(); e.HasOne(x => x.Exam).WithMany(x => x.Parts).HasForeignKey(x => x.ExamId); });
        b.Entity<ToeicPassage>(e => { e.ToTable("ReadingPassage"); e.HasKey(x => x.PassageId); e.Property(x => x.PassageCode).HasMaxLength(50).IsRequired(); e.HasOne(x => x.Part).WithMany(x => x.Passages).HasForeignKey(x => x.PartId); });
        b.Entity<ToeicQuestion>(e => { e.ToTable("Question"); e.HasKey(x => x.QuestionId); e.Property(x => x.QuestionText).IsRequired(); e.HasIndex(x => new { x.ExamId, x.QuestionNumber }).IsUnique(); e.HasOne(x => x.Exam).WithMany().HasForeignKey(x => x.ExamId); e.HasOne(x => x.Part).WithMany(x => x.Questions).HasForeignKey(x => x.PartId); e.HasOne<ToeicPassage>().WithMany().HasForeignKey(x => x.PassageId).OnDelete(DeleteBehavior.NoAction); });
        b.Entity<ToeicOption>(e => { e.ToTable("QuestionOption"); e.HasKey(x => x.OptionId); e.Property(x => x.OptionKey).HasMaxLength(1).IsRequired(); e.Property(x => x.OptionText).IsRequired(); e.HasIndex(x => new { x.QuestionId, x.OptionKey }).IsUnique(); e.HasOne(x => x.Question).WithMany(x => x.Options).HasForeignKey(x => x.QuestionId); });
    }
}
