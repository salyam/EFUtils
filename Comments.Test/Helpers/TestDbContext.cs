using Microsoft.EntityFrameworkCore;
using Salyam.EFUtils.Comments.Attributes;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace Salyam.EFUtils.Comments.Test.Helpers;

public class EntityBase
{
    public int Id { get; set; } 
}

[Commentable(typeof(IdentityUser))]
public class Book : EntityBase
{
    public required string Title { get; set; } 
    
    public required string Author { get; set; } 
}

public partial class TestDbContext : IdentityDbContext 
{
    public DbSet<Book> Books { get; set; }

    public TestDbContext() {}

    public TestDbContext(DbContextOptions options) : base(options) {}

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
    }
}