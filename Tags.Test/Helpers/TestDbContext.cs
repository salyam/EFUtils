using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Salyam.EFUtils.Tags;

namespace EFTagTest.Helpers;

public class Book
{
    public int Id { get; set; } 
    public required string Title { get; set; } 
    
    public required string Author { get; set; } 
}

public class Article
{
    public int Id { get; set; } 

    public required string Title { get; set; }

    public required string Content { get; set; } 
    
    public required string Author { get; set; } 
}

public partial class TestDbContext : IdentityDbContext, ITagDbContext<Book>, ITagDbContext<Article>
{
    public DbSet<Book> Books { get; set; }
    public DbSet<Article> Articles { get; set; }

    DbSet<ITagDbContext<Article>.TagModel> ITagDbContext<Article>.Tags 
        => Set<ITagDbContext<Article>.TagModel>();

    DbSet<ITagDbContext<Article>.TaggedEntityModel> ITagDbContext<Article>.TaggedEntities 
        => Set<ITagDbContext<Article>.TaggedEntityModel>();

    DbSet<ITagDbContext<Book>.TagModel> ITagDbContext<Book>.Tags 
        => Set<ITagDbContext<Book>.TagModel>();

    DbSet<ITagDbContext<Book>.TaggedEntityModel> ITagDbContext<Book>.TaggedEntities 
        => Set<ITagDbContext<Book>.TaggedEntityModel>();

    public TestDbContext() { }

    public TestDbContext(DbContextOptions options) : base(options) { }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);  
    }
}