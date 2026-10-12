using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Chien.EntityFrameworkCore.Toeic;

public class ToeicDbContextFactory
{
    private readonly IConfiguration _configuration;
    public ToeicDbContextFactory(IConfiguration configuration) { _configuration = configuration; }
    public ToeicDbContext Create()
    {
        var cs = _configuration.GetConnectionString("Toeic") ?? _configuration.GetConnectionString("Default");
        return new ToeicDbContext(new DbContextOptionsBuilder<ToeicDbContext>().UseSqlServer(cs).Options);
    }
}
