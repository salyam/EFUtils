using Microsoft.EntityFrameworkCore;
using Salyam.EFTag;
using Salyam.EFUtils.Tags.Attributes;

namespace EFTagTest.Helpers;

[Taggable]
public class Book
{
    public int Id { get; set; } 

    public required string Title { get; set; } 
    
    public required string Author { get; set; } 
}

public partial class TestDbContext : DbContext
{
    public DbSet<Book> Books { get; set; }

    public TestDbContext()
    {
        
    }

    public TestDbContext(DbContextOptions options) : base(options)
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
        
    }
}