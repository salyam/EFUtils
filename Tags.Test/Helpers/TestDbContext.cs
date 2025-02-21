using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Salyam.EFUtils.Tags.Attributes;

namespace EFTagTest.Helpers;

public class EntityBase
{
    public required string Id { get; set; } 
}

[Taggable]
public class Book : EntityBase
{
    public required string Title { get; set; } 
    
    public required string Author { get; set; } 
}

public partial class TestDbContext : IdentityDbContext
{
    public DbSet<Book> Books { get; set; }

    public TestDbContext()
    {}

    public TestDbContext(DbContextOptions options) : base(options)
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);  
    }
}