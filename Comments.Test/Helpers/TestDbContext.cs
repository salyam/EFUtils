using Microsoft.EntityFrameworkCore;
using Salyam.EFUtils.Comments.Attributes;

namespace Salyam.EFUtils.Comments.Test.Helpers;

[Commentable(typeof(User))]
public class Book
{
    public int Id { get; set; } 

    public required string Title { get; set; } 
    
    public required string Author { get; set; } 
}

public class User
{
    public int Id { get; set; } 

    public required string Name { get; set; }
}

public partial class TestDbContext : DbContext
{
    public DbSet<Book> Books { get; set; }
    public DbSet<User> Users { get; set; }

    public TestDbContext() {}

    public TestDbContext(DbContextOptions options) : base(options) {}

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
    }
}