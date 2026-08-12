using Microsoft.EntityFrameworkCore;

namespace Taskify.Data;

public class TaskifyDbContext : DbContext
{
    public TaskifyDbContext(DbContextOptions<TaskifyDbContext> options)
        : base(options)
    {
    }

    // DbSet<> se añadirán en la Fase 2 cuando se definan las entidades
}
