using Microsoft.EntityFrameworkCore;
using Entities;

namespace JobProcessing;

public sealed class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Job> Jobs { get; set; }

}
